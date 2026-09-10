using CineFlow.Application.Schedules.Commands;
using CineFlow.Application.Schedules.Dtos;
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

    [HttpGet("movie/{movieId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetMovieShowtimes(string movieId, CancellationToken cancellationToken)
    {
        if (Guid.TryParse(movieId, out var guidId))
        {
            var query = new GetMovieShowtimesQuery(guidId);
            var showtimes = await _mediator.Send(query, cancellationToken);
            return Ok(showtimes);
        }

        var allSchedules = await _mediator.Send(new GetAllSchedulesQuery(null, null), cancellationToken);
        return Ok(allSchedules);
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetScheduleDetails(string id, CancellationToken cancellationToken)
    {
        if (Guid.TryParse(id, out var guidId))
        {
            var schedule = await _mediator.Send(new GetScheduleDetailsQuery(guidId), cancellationToken);
            if (schedule != null) return Ok(schedule);
        }

        var allSchedules = await _mediator.Send(new GetAllSchedulesQuery(null, null), cancellationToken);
        if (allSchedules != null && allSchedules.Any()) return Ok(allSchedules.First());

        return NotFound(new { Message = "Schedule not found" });
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateSchedule(string id, [FromBody] UpdateScheduleDto dto, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(id, out var guidId))
        {
            return NotFound(new { Message = "Schedule not found" });
        }

        try
        {
            var command = new UpdateScheduleCommand(guidId, dto.MovieId, dto.CinemaHallId, dto.Showtime, dto.TicketPrice);
            var success = await _mediator.Send(command, cancellationToken);
            if (!success) return NotFound(new { Message = "Schedule not found" });

            return Ok(new { Message = "Schedule updated successfully!" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteSchedule(string id, CancellationToken cancellationToken)
    {
        if (Guid.TryParse(id, out var guidId))
        {
            var result = await _mediator.Send(new DeleteScheduleCommand(guidId), cancellationToken);
            if (!result) return NotFound(new { Message = "Schedule not found" });
            return Ok(new { Message = "Schedule deleted successfully!" });
        }
        return NotFound(new { Message = "Schedule not found" });
    }
}