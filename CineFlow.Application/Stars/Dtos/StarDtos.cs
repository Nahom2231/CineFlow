using System;
using System.Collections.Generic;

namespace CineFlow.Application.Stars.Dtos;

public class StarResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;
    public int MovieCount { get; set; }
    public List<StarMovieDto> Movies { get; set; } = new();
}

public class StarMovieDto
{
    public Guid Id { get; set; }
    public string TitleEnglish { get; set; } = string.Empty;
    public string TitleAmharic { get; set; } = string.Empty;
    public string FeaturedImageUrl { get; set; } = string.Empty;
    public string Genre { get; set; } = string.Empty;
}

public class CreateStarDto
{
    public string Name { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;
}

public class UpdateStarDto
{
    public string Name { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;
}
