using System.Data.SqlTypes;
using CineFlow.Application.Common.Interfaces;
using CineFlow.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CineFlow.Application.Tickets.Commands;

public record BookTicketCommand(
    Guid ScheduleId,
    string SeatNumber,
    string PaymentPhoneNumber,
    string PaymentProvider,
    string? UserId 
    ) : IRequest<Guid>;

    public class BookTicketCommandHandler : IRequestHandler<BookTicketCommand, Guid>
{
    private readonly ICineFlowDbContext _context;

    public BookTicketCommandHandler(ICineFlowDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(BookTicketCommand request, CancellationToken cancellationToken)
    {
        var schedule =await _context.Schedules
        .Include(s=>s.Tickets)
        .FirstOrDefaultAsync(s=> s.Id == request.ScheduleId, cancellationToken);
        if (schedule == null)
        {
            throw new Exception ("The requested showtime schedule does not exist");
        }
        if (schedule.AvailableSeats <=0)
        {
            throw new Exception("No available seats for this showtime schedule");
        }
        
        var targetSeat= request.SeatNumber.Trim().ToUpper();
        var isSeatTaken = schedule.Tickets.Any(t=>t.SeatNumber.Trim().ToUpper()== targetSeat);
        if (isSeatTaken)
        {
            throw new Exception($"Seat {request.SeatNumber} is already booked for this show");
        }

    var shortRandomSuffix= Random.Shared.Next(100000, 999999);
    var mockTxnRef = $"Txn-{request.PaymentProvider.ToUpper()}-{shortRandomSuffix}";

    schedule.AvailableSeats -=1;

    var ticket = new Ticket

    {
        Id = Guid.NewGuid(),
        ScheduleId= schedule.Id,
        SeatNumber = request.SeatNumber,
        AmountPaid = schedule.TicketPrice,
        MockTransactionReference = $"Txn-{request.PaymentProvider? .ToUpper() ?? "PAYMENT"} -{Random.Shared.Next(100000, 999999)}",
        IsUsed = false,
        PurchasedAt = DateTime.UtcNow,
        UserId = request.UserId ?? string.Empty
    };
   var activeHold = await _context.SeatReservations.FirstOrDefaultAsync(r =>
   r.ScheduleId ==request.ScheduleId&&
   r.SeatNumber.Trim().ToUpper()==targetSeat &&
   r.UserId == request.UserId&&
   !r.IsCompleted, cancellationToken);

   if(activeHold!= null)
        {
            activeHold.IsCompleted = true;
        }
    _context.Tickets.Add(ticket);

    try{
    await _context.SaveChangesAsync(cancellationToken);
    }
     catch (DbUpdateConcurrencyException)
        {
            throw new Exception("Another user just booked for this show. Please try again.");
        }
    return ticket.Id;
}
}