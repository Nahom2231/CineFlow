using CineFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CineFlow.Infrastructure.Persistence.Configurations;

public class MovieConfiguration : IEntityTypeConfiguration<Movie>
{
    public void Configure(EntityTypeBuilder<Movie> builder)
    {
        builder.ToTable("Movies");

        builder.HasKey(m => m.Id);

        
        builder.Property(m => m.TitleEnglish)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(m => m.TitleAmharic)
            .IsRequired()
            .HasMaxLength(200);

        
        builder.Property(m => m.DescriptionEnglish)
            .HasMaxLength(2000);

        builder.Property(m => m.DescriptionAmharic)
            .HasMaxLength(2000);

        builder.Property(m => m.Genre)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(m => m.AudioLanguage)
            .HasMaxLength(50);

        builder.Property(m => m.FeaturedImageUrl)
            .HasMaxLength(500);

        
        builder.HasOne(m => m.Director)
            .WithMany(d=>d.Movies)
            .HasForeignKey(m => m.DirectorId)
            .OnDelete(DeleteBehavior.Restrict);

    
        builder.HasMany(m => m.Stars)
            .WithMany(s => s.Movies)
            .UsingEntity(j => j.ToTable("MovieStars"));

        
        builder.HasMany(m => m.Schedules)
            .WithOne(s => s.Movie)
            .HasForeignKey(s => s.MovieId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}