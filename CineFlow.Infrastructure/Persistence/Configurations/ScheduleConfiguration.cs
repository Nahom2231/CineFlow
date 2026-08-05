using CineFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace CineFlow.Infrastructure.Persistence.Configurations;

public class ScheduleConfiguration : IEntityTypeConfiguration<Schedule>
{
    public void Configure(EntityTypeBuilder<Schedule> builder)
    {
        builder.ToTable("Schedules");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Showtime)
            .IsRequired();

        
        builder.Property(s => s.TicketPrice)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(s => s.AvailableSeats)
            .IsRequired();

        builder.Property(s => s.RowVersion)
            .IsRowVersion();

        builder.HasOne(s => s.CinemaHall)
              .WithMany(h=> h.Schedule)
              .HasForeignKey(s=> s.CinemaHallId)
              .OnDelete(DeleteBehavior.Restrict);

        
        builder.HasOne(s => s.Movie)
            .WithMany(m => m.Schedules)
            .HasForeignKey(s => s.MovieId)
            .OnDelete(DeleteBehavior.Cascade);

        
        builder.HasMany(s => s.Tickets)
            .WithOne(t => t.Schedule)
            .HasForeignKey(t => t.ScheduleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}