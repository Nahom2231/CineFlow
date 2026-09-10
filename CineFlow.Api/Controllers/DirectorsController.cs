using System;
using System.Threading;
using System.Threading.Tasks;
using CineFlow.Application.Directors.Commands;
using CineFlow.Application.Directors.Dtos;
using CineFlow.Application.Directors.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CineFlow.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class DirectorsController : ControllerBase
{
    private readonly IMediator _mediator;

    public DirectorsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAllDirectors(CancellationToken cancellationToken)
    {
        var directors = await _mediator.Send(new GetAllDirectorsQuery(), cancellationToken);
        return Ok(directors);
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetDirectorById(string id, CancellationToken cancellationToken)
    {
        if (Guid.TryParse(id, out var guidId))
        {
            var director = await _mediator.Send(new GetDirectorByIdQuery(guidId), cancellationToken);
            if (director != null) return Ok(director);
        }

        return NotFound(new { Message = "Director not found" });
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateDirector([FromBody] CreateDirectorDto dto, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            return BadRequest(new { Message = "Director name is required." });
        }

        var command = new CreateDirectorCommand(dto.Name, dto.Bio);
        var directorId = await _mediator.Send(command, cancellationToken);
        return Ok(new { DirectorId = directorId, Message = "Director created successfully!" });
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateDirector(string id, [FromBody] UpdateDirectorDto dto, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(id, out var guidId))
        {
            return NotFound(new { Message = "Director not found" });
        }

        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            return BadRequest(new { Message = "Director name is required." });
        }

        var command = new UpdateDirectorCommand(guidId, dto.Name, dto.Bio);
        var success = await _mediator.Send(command, cancellationToken);
        if (!success) return NotFound(new { Message = "Director not found" });

        return Ok(new { Message = "Director updated successfully!" });
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteDirector(string id, CancellationToken cancellationToken)
    {
        if (Guid.TryParse(id, out var guidId))
        {
            var success = await _mediator.Send(new DeleteDirectorCommand(guidId), cancellationToken);
            if (!success) return NotFound(new { Message = "Director not found" });

            return Ok(new { Message = "Director deleted successfully!" });
        }

        return NotFound(new { Message = "Director not found" });
    }
}
