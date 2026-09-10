using System;
using System.Collections.Generic;

namespace CineFlow.Application.Directors.Dtos;

public class DirectorResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;
    public int MovieCount { get; set; }
    public List<DirectorMovieDto> Movies { get; set; } = new();
}

public class DirectorMovieDto
{
    public Guid Id { get; set; }
    public string TitleEnglish { get; set; } = string.Empty;
    public string TitleAmharic { get; set; } = string.Empty;
    public string FeaturedImageUrl { get; set; } = string.Empty;
    public string Genre { get; set; } = string.Empty;
}

public class CreateDirectorDto
{
    public string Name { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;
}

public class UpdateDirectorDto
{
    public string Name { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;
}
