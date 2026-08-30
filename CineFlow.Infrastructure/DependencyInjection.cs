using CineFlow.Application.Common.Interfaces;
using CineFlow.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using CineFlow.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using System.Text;
using System.Security.Claims;
namespace CineFlow.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureService(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<CineFlowDbContext>(options =>
        options.UseNpgsql(connectionString));
        services.AddScoped<ICineFlowDbContext>(provider =>provider.GetRequiredService<CineFlowDbContext>());
         services.AddScoped<IFileStorageService, LocalFileStorageService>();
         services.AddScoped<IQRCodeService, QRCodeService>();
         services.AddHostedService<ExpiredReservationCleanupWorker>();
         services.AddIdentity<IdentityUser, IdentityRole>(options =>
         {
             options.Password.RequireDigit = true;
             options.Password.RequiredLength = 6;
             options.Password.RequireUppercase = false;
             options.Password.RequireLowercase = false;
             options.Password.RequireNonAlphanumeric = false;
             options.User.RequireUniqueEmail = true;

             // Account lockout configuration (5 failed attempts -> 60s cooldown rate limit)
             options.Lockout.AllowedForNewUsers = true;
             options.Lockout.MaxFailedAccessAttempts = 5;
             options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromSeconds(60);
         })
         .AddEntityFrameworkStores<CineFlowDbContext>()
         .AddDefaultTokenProviders();
         var jwtSettings = configuration.GetSection("JwtSettings");
         var secretKey = jwtSettings["Secret"] ?? "CineFlowSuperSecureEnterpriseTokenSigningPrivateKey2026";
         services.AddAuthentication(options =>
         {
             options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
             options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
         })
         .AddJwtBearer(options =>
         {
             options.RequireHttpsMetadata = false;
             options.SaveToken = true;
             options.TokenValidationParameters = new TokenValidationParameters
             {
                 ValidateIssuer = true,
                 ValidateAudience = true,
                 ValidateLifetime = true,
                 ValidateIssuerSigningKey = true,
                 ValidIssuer = jwtSettings["Issuer"] ?? "CineFlowApi",
                 ValidAudience = jwtSettings["Audience"] ?? "CineFlowAngularClient",
                 IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                 RoleClaimType = ClaimTypes.Role,
                 NameClaimType = ClaimTypes.Name,
                 ClockSkew = TimeSpan.FromMinutes(5)
             };
         });
        return services;
    }
}