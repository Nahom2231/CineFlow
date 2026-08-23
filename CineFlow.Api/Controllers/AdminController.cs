using System;
using System.Threading;
using System.Threading.Tasks;
using CineFlow.Application.Admin.Queries;
using CineFlow.Application.Tickets.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CineFlow.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Gets overall cinema statistics: total revenue, today's earnings, tickets sold, active movies, and recent bookings.
    /// </summary>
    [HttpGet("dashboard-stats")]
    public async Task<IActionResult> GetDashboardStats(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetAdminDashboardStatsQuery(), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Gets revenue breakdown by movie, ranking the most popular movies and their box office performance.
    /// </summary>
    [HttpGet("top-movies")]
    public async Task<IActionResult> GetRevenueByMovie(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetRevenueByMovieQuery(), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Gets daily revenue and ticket sales trend for the past N days (default 7 days).
    /// </summary>
    [HttpGet("weekly-revenue")]
    public async Task<IActionResult> GetDailyRevenue([FromQuery] int days = 7, CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetDailyRevenueQuery(days), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Scans a ticket QR code at the door. Prevents ticket fraud and double-entry by checking and marking ticket as used.
    /// </summary>
    [HttpPost("scan-ticket")]
    public async Task<IActionResult> ScanTicket([FromBody] ValidateTicketCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }
}
