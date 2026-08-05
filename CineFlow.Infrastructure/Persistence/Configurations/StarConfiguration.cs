using CineFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CineFlow.Infrastructure.Persistence.Configurations;

public class StarConfiguration : IEntityTypeConfiguration<Star>
{
    public void Configure(EntityTypeBuilder<Star> builder)
    {
        builder.ToTable("Stars");
        builder.HasKey(S=>S.Id);
        builder.Property(s=>s.Name)
        .IsRequired()
        .HasMaxLength(150);
        builder.Property(s=>s.Bio)
        .HasMaxLength(1000);
        
    }
}