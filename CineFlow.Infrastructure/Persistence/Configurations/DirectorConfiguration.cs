using CineFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CineFlow.Infrastructure.Persistence.Configurations;

public class DirectorConfiguration : IEntityTypeConfiguration<Director>
{
    public void Configure(EntityTypeBuilder<Director> builder)
    {
        builder.ToTable("Director");
        builder.HasKey(d =>d.Id);

        builder.Property(d=> d.Name)
        .IsRequired()
        .HasMaxLength(150);
        builder.Property(d=>d.Bio)
        .HasMaxLength(100);
    }
}