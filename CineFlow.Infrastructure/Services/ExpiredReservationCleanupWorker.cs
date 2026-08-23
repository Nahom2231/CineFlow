using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CineFlow.Application.Common.Interfaces;
using CineFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CineFlow.Infrastructure.Services;

public class ExpiredReservationCleanupWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ExpiredReservationCleanupWorker> _logger;

    public ExpiredReservationCleanupWorker(IServiceProvider serviceProvider, ILogger<ExpiredReservationCleanupWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
{
    while (!stoppingToken.IsCancellationRequested)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ICineFlowDbContext>();

            // Your cleanup logic...
            
            // Wait for next run interval
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // Expected shutdown when Ctrl+C or app stops, do nothing
            break;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred in ExpiredReservationCleanupWorker");
        }
    }
}
}