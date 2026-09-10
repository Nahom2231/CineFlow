using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CineFlow.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CineFlow.Application.Schedules.Commands;

public record UpdateScheduleCommand(
    Guid Id,
    Guid MovieId,
    Guid CinemaHallId,
    DateTime Showtime,
    decimal TicketPrice
) : IRequest<bool>;

public class UpdateScheduleCommandHandler : IRequestHandler<UpdateScheduleCommand, bool>
{
    private readonly ICineFlowDbContext _context;

    public UpdateScheduleCommandHandler(ICineFlowDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(UpdateScheduleCommand request, CancellationToken cancellationToken)
    {
        var schedule = await _context.Schedules
            .Include(s => s.Tickets)
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        if (schedule == null) return false;

        var movieExists = await _context.Movies.AnyAsync(m => m.Id == request.MovieId, cancellationToken);
        if (!movieExists)
        {
            throw new Exception("The specified movie does not exist.");
        }

        var hall = await _context.CinemaHalls.FirstOrDefaultAsync(h => h.Id == request.CinemaHallId, cancellationToken);
        if (hall == null)
        {
            throw new Exception("The specified cinema hall does not exist.");
        }

        schedule.MovieId = request.MovieId;
        schedule.CinemaHallId = request.CinemaHallId;
        schedule.Showtime = request.Showtime;
        schedule.TicketPrice = request.TicketPrice;

        var bookedTicketsCount = schedule.Tickets.Count(t => !t.IsUsed);
        schedule.AvailableSeats = Math.Max(0, hall.TotalCapacity - bookedTicketsCount);

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
