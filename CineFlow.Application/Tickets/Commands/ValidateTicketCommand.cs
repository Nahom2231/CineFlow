using CineFlow.Application.Common.Interfaces;
using MediatR;  
using Microsoft.EntityFrameworkCore;

namespace CineFlow.Application.Tickets.Commands;

public record ValidateTicketCommand(string TransactionReference) : IRequest<bool>;

public class ValidateTicketCommandHandler : IRequestHandler<ValidateTicketCommand, bool>
{
    private readonly ICineFlowDbContext _context;

    public ValidateTicketCommandHandler(ICineFlowDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(ValidateTicketCommand request, CancellationToken cancellationToken)
    {
        var ticket = await _context.Tickets
            .Include(t => t.Schedule)
            .ThenInclude(s => s!.Movie)
            .FirstOrDefaultAsync(t => t.MockTransactionReference == request.TransactionReference, cancellationToken);

        if (ticket == null)
        {
            throw new Exception("Invalid ticket reference.");
        }

        if (ticket.IsUsed)
        {
            throw new Exception("The ticket was already used on {ticket.UsedAt: g}.");
        }

        // Mark the ticket as used
        ticket.IsUsed = true;
        ticket.UsedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}