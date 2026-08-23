using CineFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace CineFlow.Application.Common.Interfaces;

public interface ICineFlowDbContext
{
    DbSet<Movie> Movies { get; }
    
    DbSet<Director> Directors { get; }

    DbSet<Star> Stars { get; }

    DbSet<CineFlow.Domain.Entities.Schedule> Schedules { get; }

    DbSet<Ticket> Tickets { get; }

    DbSet<CinemaHall> CinemaHalls { get; }

    DbSet<SeatReservation> SeatReservations { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
