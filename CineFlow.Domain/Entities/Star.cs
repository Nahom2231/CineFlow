namespace CineFlow.Domain.Entities;

public class Star
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Bio{get; set;} =string.Empty;

    public ICollection<Movie> Movies { get; set; } = new List<Movie>();
}