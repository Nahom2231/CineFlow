using CineFlow.Application.Common.Interfaces;
using CineFlow.Infrastructure.Persistence;
using CineFlow.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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
        return services;
    }
}