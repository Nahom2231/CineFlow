using CineFlow.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CineFlow.Application.Movies.Commands;

public record DeleteMovieCommand(Guid Id) : IRequest<bool>;

public class DeleteMovieCommandHandler : IRequestHandler<DeleteMovieCommand, bool>
{
    private readonly ICineFlowDbContext _context;

    public DeleteMovieCommandHandler(ICineFlowDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeleteMovieCommand request, CancellationToken cancellationToken)
    {
        var movie = await _context.Movies
            .Include(m => m.Schedules)
            .FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken);

        if (movie == null) return false;

        if (movie.Schedules.Any())
        {
            var scheduleIds = movie.Schedules.Select(s => s.Id).ToList();

            var reservations = await _context.SeatReservations
                .Where(r => scheduleIds.Contains(r.ScheduleId))
                .ToListAsync(cancellationToken);
            if (reservations.Any())
            {
                _context.SeatReservations.RemoveRange(reservations);
            }

            var tickets = await _context.Tickets
                .Where(t => scheduleIds.Contains(t.ScheduleId))
                .ToListAsync(cancellationToken);
            if (tickets.Any())
            {
                _context.Tickets.RemoveRange(tickets);
            }

            _context.Schedules.RemoveRange(movie.Schedules);
        }

        _context.Movies.Remove(movie);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
