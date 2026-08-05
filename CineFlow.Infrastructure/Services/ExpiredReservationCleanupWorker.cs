using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CineFlow.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualBasic;

namespace CineFlow.Infrastructure.Services;

public class ExpiredReservationCleanupWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    public ExpiredReservationCleanupWorker(IServiceProvider serviceProvider)
    {
        _serviceProvider =serviceProvider;
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ICineFlowDbContext>();
                var expiredReservations= await context.SeatReservations
                .Where(r=>!r.IsCompleted && r.ExpiredAt <= DateTime.UtcNow)
                .ToListAsync(stoppingToken);

                foreach (var reservation in expiredReservations)
                {
                    var schedule = await context.Schedules.FindAsync(new object[]{reservation.ScheduleId}, stoppingToken );
                    if(schedule!= null)
                    {
                        schedule.AvailableSeats+= 1;
                    }
                    reservation.IsCompleted =true;
                }
                if (expiredReservations.Any())
                {
                    await context.SaveChangesAsync(stoppingToken);
                }
            }
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }
}