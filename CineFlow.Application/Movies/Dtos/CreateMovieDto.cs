public class CreateMovieDto
{
    public string TitleEnglish {get; set;}=string.Empty;
    public string TitleAmharic {get; set;}=string.Empty;
    public string DescriptionEnglish {get; set;}=string.Empty;
    public int DurationMinutes {get; set;}

    public string Genre {get; set;}=string.Empty;
    public string AudioLanguage {get; set;}=string.Empty;
    public Guid DirectorId { get; set;}

    public string? FeaturedImageUrl {get; set; }
}