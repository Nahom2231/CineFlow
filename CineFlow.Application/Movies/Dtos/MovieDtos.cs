namespace  CineFlow.Application.Movies.Dtos;

public class MovieDto
{
    public Guid Id {get; set;}
    public string TitleEnglish {get; set;}=string.Empty;

    public string TitleAmharic {get; set;} = string.Empty;
    public string DescriptionEnglish {get; set;}=string.Empty;

    public string DescriptionAmharic {get; set;} = string.Empty;

    public int DurationMinutes {get; set;}

    public string Genre {get; set;}=string.Empty;
    public string AudioLanguage {get;set;}=string.Empty;

    public string FeaturedImageUrl {get; set;}=string.Empty;

    public List<string> GalleryImageUrls {get; set;}=new();

    public string DirectorName {get; set;} = string.Empty;

    public List <string> StarName {get; set;}= new();
}