using CineFlow.Application.Movies.Commands;
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

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetMovieById(Guid id, CancellationToken cancellationToken)
    {
        var movie = await _mediator.Send(new GetMovieByIdQuery(id), cancellationToken);
        if (movie == null) return NotFound(new { Message = "Movie not found" });
        return Ok(movie);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> CreateMovie([FromForm] CreateMovieCommand command, CancellationToken cancellationToken)
    {
        var movieId = await _mediator.Send(command, cancellationToken);
        return Ok(new { MovieId = movieId, Message = "Movie created successfully!" });
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteMovie(Guid id, CancellationToken cancellationToken)
    {
        var success = await _mediator.Send(new DeleteMovieCommand(id), cancellationToken);
        if (!success) return NotFound(new { Message = "Movie not found" });
        return Ok(new { Message = "Movie deleted successfully!" });
    }
}