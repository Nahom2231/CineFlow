using CineFlow.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CineFlow.Application.Schedules.Commands;

public record DeleteScheduleCommand(Guid Id) : IRequest<bool>;

public class DeleteScheduleCommandHandler : IRequestHandler<DeleteScheduleCommand, bool>
{
    private readonly ICineFlowDbContext _context;

    public DeleteScheduleCommandHandler(ICineFlowDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeleteScheduleCommand request, CancellationToken cancellationToken)
    {
        var schedule = await _context.Schedules
            .Include(s => s.Tickets)
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        if (schedule == null) return false;

        // If tickets are already booked, we could either cancel or disallow
        _context.Schedules.Remove(schedule);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
