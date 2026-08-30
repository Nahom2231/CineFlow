using CineFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CineFlow.Infrastructure.Persistence.Configurations;

public class CinemaHallConfiguration: IEntityTypeConfiguration<CinemaHall>{
    public void Configure(EntityTypeBuilder<CinemaHall> builder)
    {
        builder.ToTable("CinemaHalls");

        builder.HasKey(h=>h.Id);

        builder.Property(h=> h.BranchName)
        .IsRequired()
        .HasMaxLength(150);

        builder.Property(h=>h.HallName)
        .IsRequired()
        .HasMaxLength(100);
        builder.Property(h =>h.TotalCapacity)
        .IsRequired();
        builder.Property(h=>h.SeatMapMatrixJson)
        .HasColumnType("jsonb");
    
        builder.HasMany(h => h.Schedules)
            .WithOne(s => s.CinemaHall)
            .HasForeignKey(s => s.CinemaHallId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}