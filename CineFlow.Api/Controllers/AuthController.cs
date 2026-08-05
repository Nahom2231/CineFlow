using System.IdentityModel.Tokens.Jwt;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualBasic;

namespace CineFlow.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IConfiguration _configuration;

    public AuthController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _configuration = configuration;
    }
    public record RegisterRequest(string Email, string Password);
    public record LoginRequest(string Email, string Password);

    [HttpPost("register")]
    public async Task<IActionResult> Register ([FromBody] RegisterRequest request){
        var user = new IdentityUser { UserName= request.Email, Email = request.Email};
        var result= await _userManager.CreateAsync(user, request.Password);

        if(!result.Succeeded)
        return BadRequest(result.Errors);

        if(!await _roleManager.RoleExistsAsync("RegularUser"))
            await _roleManager.CreateAsync(new IdentityRole("RegularUser"));
        await _userManager.AddToRoleAsync(user, "RegularUser");
        return Ok(new {Message = "User registered successfully"});
    }
    [HttpPost("login")]
    public async Task<IActionResult> Login ([FromBody] LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if(user==null|| !await _userManager.CheckPasswordAsync(user, request.Password))
        return Unauthorized(new {Error = "Invalid username or password configuration "});
        var userRoles = await _userManager.GetRolesAsync(user);

        var authClaims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Email, user.Email)
        };
        foreach (var role in userRoles)
        {
            authClaims.Add(new Claim(ClaimTypes.Role, role));
        }
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Secret"]!));

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: authClaims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
        );
        return Ok(new
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            Expiration = token.ValidTo
        });
    }
    [HttpPost("seed-admin")]
    public async Task<IActionResult> SeedAdmin()
    {
        if(!await _roleManager.RoleExistsAsync("Admin"))
        await _roleManager.CreateAsync(new IdentityRole("Admin"));

        var adminEmail= "admin@cineflow.com";
        var existingAdmin = await _userManager.FindByEmailAsync(adminEmail);

        if (existingAdmin == null)
        {
            var adminUser = new IdentityUser { UserName = adminEmail, Email = adminEmail };
            await _userManager.CreateAsync(adminUser, "AddisAbaba2026!");
            await _userManager.AddToRoleAsync(adminUser, "Admin");
            return Ok(new { Message = "Admin account seeded successfully!Use admin@cineflow.com and AddisAbaba2026!"});
        }
        return BadRequest(new { Message = "Admin account has already been initialized on this server." });
    }
}

public static class RoleManagerExtensions
{
    public static Task<IdentityResult> CreateCreateRoleAsync(this RoleManager<IdentityRole> roleManager, IdentityRole role)=>
     roleManager.CreateAsync(role);
}