using CineFlow.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CineFlow.Application.Schedules.Queries;

public class MovieShowtimeDto
{
    public Guid ScheduleId { get; set; }
    public DateTime Showtime { get; set; }
    public decimal TicketPrice { get; set; }
    public int AvailableSeats { get; set; }
    public string CinemaBranch { get; set; } = string.Empty;
    public string CinemaHall { get; set; } = string.Empty;
}

/// <summary>
/// Query to get all available showtimes for a specific movie.
/// Customers use this to see what times they can watch a movie.
/// </summary>
public record GetMovieShowtimesQuery(Guid MovieId) : IRequest<List<MovieShowtimeDto>>;

public class GetMovieShowtimesQueryHandler : IRequestHandler<GetMovieShowtimesQuery, List<MovieShowtimeDto>>
{
    private readonly ICineFlowDbContext _context;

    public GetMovieShowtimesQueryHandler(ICineFlowDbContext context)
    {
        _context = context;
    }

    public async Task<List<MovieShowtimeDto>> Handle(GetMovieShowtimesQuery request, CancellationToken cancellationToken)
    {
        var schedules = await _context.Schedules
            .Where(s => s.MovieId == request.MovieId && s.Showtime > DateTime.UtcNow)
            .Include(s => s.CinemaHall)
            .OrderBy(s => s.Showtime)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return schedules.Select(s => new MovieShowtimeDto
        {
            ScheduleId = s.Id,
            Showtime = s.Showtime,
            TicketPrice = s.TicketPrice,
            AvailableSeats = s.AvailableSeats,
            CinemaBranch = s.CinemaHall?.BranchName ?? "Unknown",
            CinemaHall = s.CinemaHall?.HallName ?? "Unknown"
        }).ToList();
    }
}
