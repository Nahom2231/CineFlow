using System;
using System.Collections.Generic;

namespace CineFlow.Application.Movies.Dtos;

public class UpdateMovieDto
{
    public string TitleEnglish { get; set; } = string.Empty;
    public string? TitleAmharic { get; set; }
    public string DescriptionEnglish { get; set; } = string.Empty;
    public string? DescriptionAmharic { get; set; }
    public int DurationMinutes { get; set; }
    public string Genre { get; set; } = string.Empty;
    public string AudioLanguage { get; set; } = string.Empty;
    public Guid? DirectorId { get; set; }
    public List<Guid>? StarIds { get; set; }
    public string? FeaturedImageUrl { get; set; }
    public List<string>? GalleryImageUrls { get; set; }
}
