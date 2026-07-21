using CineFlow.Application.Common.Interfaces;
using CineFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CineFlow.Infrastructure.Persistence;

public class CineFlowDbContext : DbContext, ICineFlowDbContext
{
    public CineFlowDbContext(DbContextOptions<CineFlowDbContext> options) : base(options)
    {
        
    }
    public DbSet<Movie> Movies => Set<Movie>();
    public DbSet<Director> Directors=> Set<Director>();

    public DbSet<Star> Stars =>Set<Star>();

    public DbSet<Schedule> Schedules =>Set<Schedule>();

    public DbSet<Ticket> Tickets =>Set<Ticket>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Schedule>()
        .Property(s=>s.TicketPrice)
        .HasPrecision(18, 2);

        modelBuilder.Entity<Ticket>()
        .Property(t=> t.AmountPaid)
        .HasPrecision(18, 2);

        modelBuilder.Entity<Movie>()
        .HasMany(m =>m.Stars)
        .WithMany(s=>s.Movies);
    }
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return base.SaveChangesAsync(cancellationToken);
    }
}