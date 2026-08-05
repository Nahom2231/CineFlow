using CineFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CineFlow.Infrastructure.Persistence.Configurations;

public class SeatReservationConfiguration : IEntityTypeConfiguration<SeatReservation>
{
    public void Configure(EntityTypeBuilder<SeatReservation> builder)
    {
        builder.ToTable("SeatReservation");

        builder.HasKey(r=>r.Id);

        builder.Property(r=> r.SeatNumber)

            .IsRequired()
            .HasMaxLength(10);

            builder.Property(r=>r.UserId)
              .IsRequired();
              builder.Property(r=>r.ExpiredAt)
              .IsRequired();
              builder.HasOne(r=>r.Schedule)
               .WithMany()
              .HasForeignKey(r=>r.ScheduleId)
              .OnDelete(DeleteBehavior.Cascade);

    }
}