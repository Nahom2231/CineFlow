using CineFlow.Application.Schedules.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
namespace CineFlow.Api.Controllers;

[ApiController]

[Route("api/v1/[controller]")]

public class ScheduleController : ControllerBase
{
    private readonly ISender _mediator;

    public ScheduleController(ISender mediator)
    {
        _mediator = mediator;
    }

    [Authorize(Roles = "Admin")]

    [HttpPost]
    public async Task<IActionResult> CreateSchedule([FromBody] CreateScheduleCommand command, CancellationToken cancellationToken)
    {
        var scheduleId = await _mediator.Send(command, cancellationToken);
        return Ok(new{ScheduleId=scheduleId, Message="Schedule created successfully"});
    }
}