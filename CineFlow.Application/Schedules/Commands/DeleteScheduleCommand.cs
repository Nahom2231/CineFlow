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

        var reservations = await _context.SeatReservations
            .Where(r => r.ScheduleId == request.Id)
            .ToListAsync(cancellationToken);
        if (reservations.Any())
        {
            _context.SeatReservations.RemoveRange(reservations);
        }

        _context.Schedules.Remove(schedule);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
