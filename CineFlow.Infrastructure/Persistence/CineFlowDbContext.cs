using System.Reflection;
using CineFlow.Application.Common.Interfaces;
using CineFlow.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;

namespace CineFlow.Infrastructure.Persistence;

public class CineFlowDbContext : IdentityDbContext<IdentityUser>, ICineFlowDbContext
{
    public CineFlowDbContext(DbContextOptions<CineFlowDbContext> options) : base(options)
    {
        
    }
    public DbSet<Movie> Movies => Set<Movie>();
    public DbSet<Director> Directors=> Set<Director>();

    public DbSet<Star> Stars =>Set<Star>();

    public DbSet<Schedule> Schedules =>Set<Schedule>();

    public DbSet<Ticket> Tickets =>Set<Ticket>();

    public DbSet<CinemaHall> CinemaHalls =>Set<CinemaHall>();

    public DbSet<SeatReservation> SeatReservations => Set<SeatReservation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return base.SaveChangesAsync(cancellationToken);
    }
}