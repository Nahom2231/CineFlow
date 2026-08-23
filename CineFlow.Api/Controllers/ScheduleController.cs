using CineFlow.Application.Schedules.Commands;
using CineFlow.Application.Schedules.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CineFlow.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class ScheduleController : ControllerBase
{
    private readonly IMediator _mediator;

    public ScheduleController(IMediator mediator)
    {
        _mediator = mediator;
    }

   [HttpPost]
[Authorize(Roles = "Admin")]
public async Task<IActionResult> CreateSchedule([FromBody] CreateScheduleCommand command, CancellationToken cancellationToken)
{
    var scheduleId = await _mediator.Send(command, cancellationToken);
    return Ok(new { ScheduleId = scheduleId, Message = "Schedule created successfully!" });
}

    [HttpGet("all")]
    [AllowAnonymous]
    public async Task<IActionResult> GetAllSchedules(
        [FromQuery] Guid? movieId,
        [FromQuery] string? cinemaBranch,
        CancellationToken cancellationToken)
    {
        var query = new GetAllSchedulesQuery(movieId, cinemaBranch);
        var schedules = await _mediator.Send(query, cancellationToken);
        return Ok(schedules);
    }

    [HttpGet("movie/{movieId:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetMovieShowtimes(Guid movieId, CancellationToken cancellationToken)
    {
        var query = new GetMovieShowtimesQuery(movieId);
        var showtimes = await _mediator.Send(query, cancellationToken);
        return Ok(showtimes);
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetScheduleDetails(Guid id, CancellationToken cancellationToken)
    {
        var schedule = await _mediator.Send(new GetScheduleDetailsQuery(id), cancellationToken);
        if (schedule == null) return NotFound(new { Message = "Schedule not found" });
        return Ok(schedule);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteSchedule(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new DeleteScheduleCommand(id), cancellationToken);
        if (!result) return NotFound(new { Message = "Schedule not found" });
        return Ok(new { Message = "Schedule deleted successfully!" });
    }
}