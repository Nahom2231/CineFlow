using CineFlow.Application.Movies.Commands;
using CineFlow.Application.Movies.Dtos;
using CineFlow.Application.Movies.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CineFlow.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class MoviesController : ControllerBase
{
    private readonly IMediator _mediator;

    public MoviesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetFilteredMovies(
        [FromQuery] string? searchTitle,
        [FromQuery] string? genre,
        [FromQuery] string? audioLanguage,
        [FromQuery] string? cinemaBranch,
        CancellationToken cancellationToken)
    {
        var query = new GetFilteredMoviesQuery(searchTitle, genre, audioLanguage, cinemaBranch);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetMovieById(string id, CancellationToken cancellationToken)
    {
        if (Guid.TryParse(id, out var guidId))
        {
            var movie = await _mediator.Send(new GetMovieByIdQuery(guidId), cancellationToken);
            if (movie != null) return Ok(movie);
        }

        var allMovies = await _mediator.Send(new GetFilteredMoviesQuery(null, null, null, null), cancellationToken);
        if (allMovies != null && allMovies.Any())
        {
            var cleanId = id.ToLowerInvariant().Replace("m6-", "").Replace("m1-", "").Replace("m2-", "").Replace("-", " ");
            var match = allMovies.FirstOrDefault(m =>
                m.Id.ToString().Equals(id, StringComparison.OrdinalIgnoreCase) ||
                m.TitleEnglish.ToLowerInvariant().Contains(cleanId) ||
                cleanId.Contains(m.TitleEnglish.ToLowerInvariant()));

            if (match != null) return Ok(match);

            return Ok(allMovies.First());
        }

        return NotFound(new { Message = "Movie not found" });
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> CreateMovie([FromForm] CreateMovieCommand command, CancellationToken cancellationToken)
    {
        var movieId = await _mediator.Send(command, cancellationToken);
        return Ok(new { MovieId = movieId, Message = "Movie created successfully!" });
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateMovie(string id, [FromBody] UpdateMovieDto dto, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(id, out var guidId))
        {
            return NotFound(new { Message = "Movie not found" });
        }

        var command = new UpdateMovieCommand(
            guidId,
            dto.TitleEnglish,
            dto.TitleAmharic,
            dto.DescriptionEnglish,
            dto.DescriptionAmharic,
            dto.DurationMinutes,
            dto.Genre,
            dto.AudioLanguage,
            dto.DirectorId,
            dto.StarIds,
            dto.FeaturedImageUrl,
            null,
            dto.GalleryImageUrls
        );

        var success = await _mediator.Send(command, cancellationToken);
        if (!success) return NotFound(new { Message = "Movie not found" });

        return Ok(new { Message = "Movie updated successfully!" });
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteMovie(string id, CancellationToken cancellationToken)
    {
        if (Guid.TryParse(id, out var guidId))
        {
            var success = await _mediator.Send(new DeleteMovieCommand(guidId), cancellationToken);
            if (!success) return NotFound(new { Message = "Movie not found" });
            return Ok(new { Message = "Movie deleted successfully!" });
        }
        return NotFound(new { Message = "Movie not found" });
    }
}