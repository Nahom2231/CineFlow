using System;
using System.Threading;
using System.Threading.Tasks;
using CineFlow.Application.Stars.Commands;
using CineFlow.Application.Stars.Dtos;
using CineFlow.Application.Stars.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CineFlow.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class StarsController : ControllerBase
{
    private readonly IMediator _mediator;

    public StarsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAllStars(CancellationToken cancellationToken)
    {
        var stars = await _mediator.Send(new GetAllStarsQuery(), cancellationToken);
        return Ok(stars);
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetStarById(string id, CancellationToken cancellationToken)
    {
        if (Guid.TryParse(id, out var guidId))
        {
            var star = await _mediator.Send(new GetStarByIdQuery(guidId), cancellationToken);
            if (star != null) return Ok(star);
        }

        return NotFound(new { Message = "Star not found" });
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateStar([FromBody] CreateStarDto dto, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            return BadRequest(new { Message = "Star name is required." });
        }

        var command = new CreateStarCommand(dto.Name, dto.Bio);
        var starId = await _mediator.Send(command, cancellationToken);
        return Ok(new { StarId = starId, Message = "Star created successfully!" });
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateStar(string id, [FromBody] UpdateStarDto dto, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(id, out var guidId))
        {
            return NotFound(new { Message = "Star not found" });
        }

        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            return BadRequest(new { Message = "Star name is required." });
        }

        var command = new UpdateStarCommand(guidId, dto.Name, dto.Bio);
        var success = await _mediator.Send(command, cancellationToken);
        if (!success) return NotFound(new { Message = "Star not found" });

        return Ok(new { Message = "Star updated successfully!" });
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteStar(string id, CancellationToken cancellationToken)
    {
        if (Guid.TryParse(id, out var guidId))
        {
            var success = await _mediator.Send(new DeleteStarCommand(guidId), cancellationToken);
            if (!success) return NotFound(new { Message = "Star not found" });

            return Ok(new { Message = "Star deleted successfully!" });
        }

        return NotFound(new { Message = "Star not found" });
    }
}
