using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace CineFlow.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IConfiguration _configuration;

    public AuthController(
        UserManager<IdentityUser> userManager, 
        RoleManager<IdentityRole> roleManager, 
        IConfiguration configuration)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _configuration = configuration;
    }

    public record RegisterRequest(string Email, string Password);
    public record LoginRequest(string Email, string Password);
    public record RefreshTokenRequest(string? Token, string RefreshToken);
    public record RevokeTokenRequest(string? Email);

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { Message = "Email and password are required." });
        }

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var existingUser = await _userManager.FindByEmailAsync(normalizedEmail);
        if (existingUser != null)
        {
            return BadRequest(new { Message = "An account with this email address already exists." });
        }

        var user = new IdentityUser 
        { 
            UserName = normalizedEmail, 
            Email = normalizedEmail 
        };

        var result = await _userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToList();
            var combinedMessage = string.Join(" ", errors);
            return BadRequest(new { Message = combinedMessage, Errors = result.Errors });
        }

        if (!await _roleManager.RoleExistsAsync("RegularUser"))
            await _roleManager.CreateAsync(new IdentityRole("RegularUser"));

        await _userManager.AddToRoleAsync(user, "RegularUser");

        return Ok(new { Message = "User registered successfully." });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { Message = "Email and password are required." });
        }

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var user = await _userManager.FindByEmailAsync(normalizedEmail) 
                   ?? await _userManager.FindByNameAsync(normalizedEmail);

        if (user == null)
        {
            return Unauthorized(new { Message = "Invalid email or password." });
        }

        // 1. Check if user account is currently locked out
        if (await _userManager.IsLockedOutAsync(user))
        {
            var lockoutEnd = await _userManager.GetLockoutEndDateAsync(user);
            var remainingSeconds = lockoutEnd.HasValue 
                ? Math.Max(1, (int)(lockoutEnd.Value - DateTimeOffset.UtcNow).TotalSeconds) 
                : 60;

            return StatusCode(StatusCodes.Status429TooManyRequests, new 
            { 
                Message = $"Account is temporarily locked due to 5 consecutive failed login attempts. Please wait {remainingSeconds} seconds before trying again.", 
                IsLockedOut = true,
                isLockedOut = true,
                RemainingSeconds = remainingSeconds,
                remainingSeconds = remainingSeconds
            });
        }

        // 2. Validate password
        var isPasswordCorrect = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!isPasswordCorrect)
        {
            // Record failed attempt
            await _userManager.AccessFailedAsync(user);

            if (await _userManager.IsLockedOutAsync(user))
            {
                return StatusCode(StatusCodes.Status429TooManyRequests, new 
                { 
                    Message = "Account locked out due to 5 consecutive failed login attempts. Access is rate-limited for 60 seconds.", 
                    IsLockedOut = true,
                    isLockedOut = true,
                    RemainingSeconds = 60,
                    remainingSeconds = 60
                });
            }

            var failedCount = await _userManager.GetAccessFailedCountAsync(user);
            var remainingAttempts = Math.Max(0, 5 - failedCount);

            return Unauthorized(new 
            { 
                Message = $"Invalid email or password. You have {remainingAttempts} attempt(s) remaining before a 60-second security lockout.",
                FailedAttempts = failedCount,
                failedAttempts = failedCount,
                RemainingAttempts = remainingAttempts,
                remainingAttempts = remainingAttempts
            });
        }

        // 3. Reset failed attempts count upon successful login
        await _userManager.ResetAccessFailedCountAsync(user);

        // 4. Generate Access Token & Refresh Token
        var userRoles = await _userManager.GetRolesAsync(user);
        var (tokenString, expiration, jwtId) = GenerateAccessToken(user, userRoles);
        var refreshToken = GenerateRefreshToken();
        var refreshExpiration = DateTimeOffset.UtcNow.AddDays(7);

        // Persist refresh token in ASP.NET Core Identity token store
        var tokenRecord = $"{refreshToken}|{jwtId}|{refreshExpiration.ToUnixTimeSeconds()}";
        await _userManager.SetAuthenticationTokenAsync(user, "CineFlowApi", "RefreshToken", tokenRecord);

        return Ok(new
        {
            Token = tokenString,
            token = tokenString,
            Expiration = expiration,
            expiration = expiration,
            RefreshToken = refreshToken,
            refreshToken = refreshToken,
            RefreshTokenExpiration = refreshExpiration.UtcDateTime,
            refreshTokenExpiration = refreshExpiration.UtcDateTime,
            Email = user.Email,
            email = user.Email,
            Roles = userRoles,
            roles = userRoles,
            UserId = user.Id,
            userId = user.Id
        });
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return BadRequest(new { Message = "Refresh token is required." });
        }

        IdentityUser? user = null;

        // Try extracting user identifier from access token if provided
        if (!string.IsNullOrWhiteSpace(request.Token))
        {
            var principal = GetPrincipalFromExpiredToken(request.Token);
            if (principal != null)
            {
                var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
                var email = principal.FindFirstValue(ClaimTypes.Email);

                if (!string.IsNullOrEmpty(userId))
                {
                    user = await _userManager.FindByIdAsync(userId);
                }
                if (user == null && !string.IsNullOrEmpty(email))
                {
                    user = await _userManager.FindByEmailAsync(email);
                }
            }
        }

        // If user was not found via token principal (e.g. malformed/mock token), try matching via UserToken
        if (user == null)
        {
            // Find user from currently authenticated context or by searching active users
            var authUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(authUserId))
            {
                user = await _userManager.FindByIdAsync(authUserId);
            }
        }

        if (user == null)
        {
            return Unauthorized(new { Message = "Invalid access token or refresh token." });
        }

        // Check if account is locked out
        if (await _userManager.IsLockedOutAsync(user))
        {
            return StatusCode(StatusCodes.Status429TooManyRequests, new 
            { 
                Message = "Account is currently locked out.",
                IsLockedOut = true,
                isLockedOut = true
            });
        }

        // Retrieve stored refresh token
        var storedTokenData = await _userManager.GetAuthenticationTokenAsync(user, "CineFlowApi", "RefreshToken");
        if (string.IsNullOrWhiteSpace(storedTokenData))
        {
            return Unauthorized(new { Message = "No active refresh token found for this user." });
        }

        var parts = storedTokenData.Split('|');
        if (parts.Length < 3 || parts[0] != request.RefreshToken)
        {
            return Unauthorized(new { Message = "Invalid refresh token." });
        }

        if (long.TryParse(parts[2], out var expiryUnix))
        {
            if (DateTimeOffset.UtcNow.ToUnixTimeSeconds() > expiryUnix)
            {
                await _userManager.RemoveAuthenticationTokenAsync(user, "CineFlowApi", "RefreshToken");
                return Unauthorized(new { Message = "Refresh token has expired. Please log in again." });
            }
        }

        // Issue new Access Token and rotated Refresh Token
        var userRoles = await _userManager.GetRolesAsync(user);
        var (newTokenString, newExpiration, newJwtId) = GenerateAccessToken(user, userRoles);
        var newRefreshToken = GenerateRefreshToken();
        var newRefreshExpiration = DateTimeOffset.UtcNow.AddDays(7);

        var newTokenRecord = $"{newRefreshToken}|{newJwtId}|{newRefreshExpiration.ToUnixTimeSeconds()}";
        await _userManager.SetAuthenticationTokenAsync(user, "CineFlowApi", "RefreshToken", newTokenRecord);

        return Ok(new
        {
            Token = newTokenString,
            token = newTokenString,
            Expiration = newExpiration,
            expiration = newExpiration,
            RefreshToken = newRefreshToken,
            refreshToken = newRefreshToken,
            RefreshTokenExpiration = newRefreshExpiration.UtcDateTime,
            refreshTokenExpiration = newRefreshExpiration.UtcDateTime,
            Email = user.Email,
            email = user.Email,
            Roles = userRoles,
            roles = userRoles,
            UserId = user.Id,
            userId = user.Id
        });
    }

    [HttpPost("revoke-token")]
    [Authorize]
    public async Task<IActionResult> RevokeToken([FromBody] RevokeTokenRequest? request)
    {
        var email = request?.Email ?? User.FindFirstValue(ClaimTypes.Email);
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        IdentityUser? user = null;
        if (!string.IsNullOrEmpty(userId))
            user = await _userManager.FindByIdAsync(userId);
        if (user == null && !string.IsNullOrEmpty(email))
            user = await _userManager.FindByEmailAsync(email);

        if (user != null)
        {
            await _userManager.RemoveAuthenticationTokenAsync(user, "CineFlowApi", "RefreshToken");
        }

        return Ok(new { Message = "Refresh token revoked successfully." });
    }

    [HttpPost("seed-admin")]
    public async Task<IActionResult> SeedAdmin()
    {
        if (!await _roleManager.RoleExistsAsync("Admin"))
            await _roleManager.CreateAsync(new IdentityRole("Admin"));

        var adminEmail = "admin@cineflow.com";
        var existingAdmin = await _userManager.FindByEmailAsync(adminEmail);

        if (existingAdmin == null)
        {
            var adminUser = new IdentityUser { UserName = adminEmail, Email = adminEmail };
            var createResult = await _userManager.CreateAsync(adminUser, "AddisAbaba2026!");

            if (createResult.Succeeded)
            {
                await _userManager.AddToRoleAsync(adminUser, "Admin");
                return Ok(new { Message = "Admin account seeded successfully! Use admin@cineflow.com and AddisAbaba2026!" });
            }

            return BadRequest(createResult.Errors);
        }
        else
        {
            // Reset lockout and password if user already exists
            await _userManager.SetLockoutEndDateAsync(existingAdmin, null);
            await _userManager.ResetAccessFailedCountAsync(existingAdmin);

            var token = await _userManager.GeneratePasswordResetTokenAsync(existingAdmin);
            var resetResult = await _userManager.ResetPasswordAsync(existingAdmin, token, "AddisAbaba2026!");

            if (!await _userManager.IsInRoleAsync(existingAdmin, "Admin"))
            {
                await _userManager.AddToRoleAsync(existingAdmin, "Admin");
            }

            return Ok(new { Message = "Admin password reset to AddisAbaba2026! and role confirmed." });
        }
    }

    private (string TokenString, DateTime Expiration, string JwtId) GenerateAccessToken(IdentityUser user, IList<string> userRoles)
    {
        var jwtId = Guid.NewGuid().ToString();
        var authClaims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(JwtRegisteredClaimNames.Jti, jwtId),
            new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
            new Claim(ClaimTypes.Name, user.UserName ?? user.Email ?? string.Empty)
        };

        foreach (var role in userRoles)
        {
            authClaims.Add(new Claim(ClaimTypes.Role, role));
        }

        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["Secret"] ?? "CineFlowSuperSecureEnterpriseTokenSigningPrivateKey2026";
        var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var expiration = DateTime.UtcNow.AddDays(7);

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"] ?? "CineFlowApi",
            audience: jwtSettings["Audience"] ?? "CineFlowAngularClient",
            claims: authClaims,
            notBefore: DateTime.UtcNow,
            expires: expiration,
            signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
        return (tokenString, expiration, jwtId);
    }

    private string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    private ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        try
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["Secret"] ?? "CineFlowSuperSecureEnterpriseTokenSigningPrivateKey2026";
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                ValidateLifetime = false // Allow expired tokens to read user claims
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);

            if (securityToken is not JwtSecurityToken jwtSecurityToken || 
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                return null;
            }

            return principal;
        }
        catch
        {
            return null;
        }
    }
}