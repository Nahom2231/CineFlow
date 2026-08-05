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
         services.AddHostedService<ExpiredReservationCleanupWorker>();
         services.AddIdentity<IdentityUser, IdentityRole>(options =>
         {
             options.Password.RequireDigit =true;
             options.Password.RequiredLength =6;
             options.Password.RequireUppercase = false;
         })
         .AddEntityFrameworkStores<CineFlowDbContext>()
         .AddDefaultTokenProviders();
         var jwtSettings= configuration.GetSection("JwtSettings");
         var secretKey= jwtSettings["Secret"]?? "SuperSecretCinemaKeyThatIsVeryLongAndSecure123";
         services.AddAuthentication(options =>
         {
             options.DefaultAuthenticateScheme =JwtBearerDefaults.AuthenticationScheme;
             options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
         })
         .AddJwtBearer(options =>
         {
             options.TokenValidationParameters = new TokenValidationParameters
             {
                 ValidateIssuer= true,
                 ValidateAudience = true,
                 ValidateLifetime =true,
                 ValidateIssuerSigningKey=true,
                 ValidIssuer = jwtSettings["Audience"],
                 IssuerSigningKey =new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
             };
         });
        return services;
    }
}