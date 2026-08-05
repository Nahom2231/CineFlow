using CineFlow.Application.Common.Interfaces;
using CineFlow.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CineFlow.Application.Schedules.Commands;

public record CreateScheduleCommand(
    Guid MovieId,
    Guid CinemaHallId,
    DateTime Showtime,
    decimal TicketPrice) : IRequest<Guid>;

public class CreateScheduleCommandHandler : IRequestHandler<CreateScheduleCommand, Guid>
{
    private readonly ICineFlowDbContext _context;

    public CreateScheduleCommandHandler(ICineFlowDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateScheduleCommand request, CancellationToken cancellationToken)
    {
        var movieExists = await _context.Movies.AnyAsync(m => m.Id == request.MovieId, cancellationToken);
        if (!movieExists)
        {
            throw new Exception("The specified movie does not exist.");
        }

        var hall = await _context.CinemaHalls.Where(h => h.Id == request.CinemaHallId)
            .FirstOrDefaultAsync(cancellationToken);
        if (hall == null)
        {
            throw new Exception("The specified cinema hall does not exist.");
        }

        var schedule = new Schedule
        {
            Id = Guid.NewGuid(),
            MovieId = request.MovieId,
            CinemaHallId = request.CinemaHallId,
            Showtime = request.Showtime,
            TicketPrice = request.TicketPrice,
            AvailableSeats = hall.TotalCapacity
        };

        _context.Schedules.Add(schedule);
        await _context.SaveChangesAsync(cancellationToken);

        return schedule.Id;
    }
}