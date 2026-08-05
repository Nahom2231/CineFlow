using System.Security.Claims;
using CineFlow.Application.Tickets.Commands;
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
    public async Task<IActionResult> BookTicket([FromBody] BookTicketCommand dto, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
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
    [Authorize(Roles = "Admin")]
    [HttpPost("validate")]
    public async Task<IActionResult> ValidateTicket([FromBody] ValidateTicketCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(new { Success = result, Message = "Ticket verified! Customer allowed entry!" });
    }
    [HttpPost("hold")]
    public async Task<IActionResult> HoldSeat([FromBody] HoldSeatCommand command)
    {
        var reservationId= await _mediator.Send(command);
        return Ok(new {ReservationId = reservationId, Message = "Seat held for 10 minutes."});
    }
}