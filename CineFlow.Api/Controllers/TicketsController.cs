using System.Security.Claims;
using CineFlow.Application.Tickets.Commands;
using CineFlow.Application.Tickets.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
namespace CIneFlow.Api.Controllers;

[ApiController]

[Route("api/v1/[controller]")]

public class TicketsController : ControllerBase
{
    private readonly IMediator _mediator;

    public TicketsController(IMediator mediator)
    {
        _mediator = mediator;
    }
    [Authorize]
    [HttpPost("book")]
    [AllowAnonymous]
    public async Task<IActionResult> BookTicket([FromBody] BookTicketCommand dto, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)?? "test-user-guid";
        var command = new BookTicketCommand(
             dto.ScheduleId,
            dto.SeatNumber,
             dto.PaymentPhoneNumber,
            dto.PaymentProvider,
            userId
        );

        var ticketId = await _mediator.Send(command, cancellationToken);
        return Ok(new { TicketId = ticketId, Message = "Ticket Successfully booked!" });
    }
    [HttpPost("validate")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ValidateTicket([FromBody] ValidateTicketCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }
    [HttpPost("hold")]
    public async Task<IActionResult> HoldSeat([FromBody] HoldSeatCommand command)
    {
        var reservationId= await _mediator.Send(command);
        return Ok(new {ReservationId = reservationId, Message = "Seat held for 10 minutes."});
    }

    /// <summary>
    /// Gets available seats for a specific movie showing.
    /// Returns an interactive seat map that customers can browse and select from.
    /// </summary>
    [AllowAnonymous]
    [HttpGet("{scheduleId:guid}/seats")]
    public async Task<IActionResult> GetAvailableSeats(Guid scheduleId, CancellationToken cancellationToken)
    {
        var query = new GetAvailableSeatsQuery(scheduleId);
        var seatsMap = await _mediator.Send(query, cancellationToken);
        return Ok(seatsMap);
    }

    /// <summary>
    /// Gets a ticket with QR code for digital pass.
    /// Customers use this to display their ticket with QR code at entry.
    /// </summary>
    [Authorize]
    [HttpGet("{ticketId:guid}")]
    public async Task<IActionResult> GetTicketWithQR(Guid ticketId, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "test-user-guid";
        var query = new GetTicketWithQRQuery(ticketId, userId);
        var ticket = await _mediator.Send(query, cancellationToken);
        return Ok(ticket);
    }
    [HttpGet("my-bookings")]
[Authorize]
public async Task<IActionResult> GetMyBookings(CancellationToken cancellationToken)
{
    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "test-user-guid";
    var query = new GetUserBookingsQuery(userId);
    var bookings = await _mediator.Send(query, cancellationToken);
    
    return Ok(bookings);
}
}