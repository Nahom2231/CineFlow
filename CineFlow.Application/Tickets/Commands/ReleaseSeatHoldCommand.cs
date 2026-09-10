using System;
using System.Threading;
using System.Threading.Tasks;
using CineFlow.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CineFlow.Application.Tickets.Commands;

public record ReleaseSeatHoldCommand(Guid ReservationId, string? UserId = null) : IRequest<bool>;

public class ReleaseSeatHoldCommandHandler : IRequestHandler<ReleaseSeatHoldCommand, bool>
{
    private readonly ICineFlowDbContext _context;

    public ReleaseSeatHoldCommandHandler(ICineFlowDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(ReleaseSeatHoldCommand request, CancellationToken cancellationToken)
    {
        var reservation = await _context.SeatReservations
            .Include(r => r.Schedule)
            .FirstOrDefaultAsync(r => r.Id == request.ReservationId, cancellationToken);

        if (reservation == null) return false;

        // If not completed, restore available seat
        if (!reservation.IsCompleted && reservation.Schedule != null)
        {
            reservation.Schedule.AvailableSeats += 1;
        }

        _context.SeatReservations.Remove(reservation);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
