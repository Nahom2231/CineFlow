using System;
using System.Threading;
using System.Threading.Tasks;
using CineFlow.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CineFlow.Application.Tickets.Commands;

public record CancelTicketCommand(Guid TicketId, string? UserId = null, bool IsAdmin = false) : IRequest<bool>;

public class CancelTicketCommandHandler : IRequestHandler<CancelTicketCommand, bool>
{
    private readonly ICineFlowDbContext _context;

    public CancelTicketCommandHandler(ICineFlowDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(CancelTicketCommand request, CancellationToken cancellationToken)
    {
        var ticket = await _context.Tickets
            .Include(t => t.Schedule)
            .FirstOrDefaultAsync(t => t.Id == request.TicketId, cancellationToken);

        if (ticket == null) return false;

        // If not admin, ensure the ticket belongs to the user
        if (!request.IsAdmin && !string.IsNullOrEmpty(request.UserId) && ticket.UserId != request.UserId)
        {
            throw new UnauthorizedAccessException("You are not authorized to cancel this ticket.");
        }

        if (ticket.IsUsed)
        {
            throw new InvalidOperationException("Cannot cancel a ticket that has already been scanned/used.");
        }

        if (ticket.Schedule != null)
        {
            ticket.Schedule.AvailableSeats += 1;
        }

        _context.Tickets.Remove(ticket);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
