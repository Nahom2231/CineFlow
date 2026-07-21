namespace CineFlow.Domain.Entities;

public class Movie
{
    public Guid Id { get; set; }
    public string TitleEnglish { get; set; } = string.Empty;
    public string TitleAmharic { get; set; } = string.Empty;
    public string DescriptionEnglish { get; set; } = string.Empty;
    public string DescriptionAmharic { get; set; } = string.Empty;
    public int DurationMinutes { get; set; }
    public string Genre { get; set; } = string.Empty;
    public string AudioLanguage { get; set; } = string.Empty;

    public string FeaturedImageUrl { get; set; } = string.Empty;
    public List<string> GalleryImageUrls { get; set; } = new();

    public Guid DirectorId { get; set; }
    public Director? Director { get; set; }

    public ICollection<Star> Stars { get; set; } = new List<Star>();
    public ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
}