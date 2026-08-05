using CineFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CineFlow.Infrastructure.Persistence.Configurations;

public class TicketConfiguration : IEntityTypeConfiguration<Ticket>
{
    public void Configure(EntityTypeBuilder<Ticket> builder)
    {
        builder.ToTable("Tickets");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.SeatNumber)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(t => t.AmountPaid)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(t => t.MockTransactionReference)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(t => t.PurchasedAt)
            .IsRequired();

        builder.Property(t => t.UserId)
            .IsRequired();

        // Foreign key relationship with Schedule
        builder.HasOne(t => t.Schedule)
            .WithMany(s => s.Tickets)
            .HasForeignKey(t => t.ScheduleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}