using CineFlow.Application.Movies.Queries;

using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CineFlow.Api.Controllers;

[ApiController]

[Route("api/v1/[controller]")]
public class MoviesController : ControllerBase
{
    private readonly IMediator _mediator;

    public  MoviesController(IMediator mediator)
    {
        _mediator = mediator;
    }
    [HttpGet]
    public async Task<IActionResult> GetFilteredMovies(
        [FromQuery] string? searchTitle,
        [FromQuery] string? genre,
        [FromQuery] string? audioLanguage,
        [FromQuery] string? cinemaBranch,
        CancellationToken cancellationToken)
    {
        var query = new  GetFilteredMoviesQuery(searchTitle, genre, audioLanguage, cinemaBranch);
        var result = await _mediator.Send(query, cancellationToken);

        return Ok(result);
    }
}