using CineFlow.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CineFlow.Application.Schedules.Queries;

public class ScheduleDetailsDto
{
    public Guid Id { get; set; }
    public Guid MovieId { get; set; }
    public string MovieTitle { get; set; } = string.Empty;
    public string MovieTitleAmharic { get; set; } = string.Empty;
    public string MovieGenre { get; set; } = string.Empty;
    public int MovieDurationMinutes { get; set; }
    public string FeaturedImageUrl { get; set; } = string.Empty;
    public Guid CinemaHallId { get; set; }
    public string HallName { get; set; } = string.Empty;
    public string BranchName { get; set; } = string.Empty;
    public string SeatMapMatrixJson { get; set; } = string.Empty;
    public DateTime Showtime { get; set; }
    public decimal TicketPrice { get; set; }
    public int AvailableSeats { get; set; }
    public int TotalCapacity { get; set; }
    public List<string> BookedSeats { get; set; } = new();
}

public record GetScheduleDetailsQuery(Guid Id) : IRequest<ScheduleDetailsDto?>;

public class GetScheduleDetailsQueryHandler : IRequestHandler<GetScheduleDetailsQuery, ScheduleDetailsDto?>
{
    private readonly ICineFlowDbContext _context;

    public GetScheduleDetailsQueryHandler(ICineFlowDbContext context)
    {
        _context = context;
    }

    public async Task<ScheduleDetailsDto?> Handle(GetScheduleDetailsQuery request, CancellationToken cancellationToken)
    {
        var s = await _context.Schedules
            .Include(s => s.Movie)
            .Include(s => s.CinemaHall)
            .Include(s => s.Tickets)
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        if (s == null) return null;

        return new ScheduleDetailsDto
        {
            Id = s.Id,
            MovieId = s.MovieId,
            MovieTitle = s.Movie?.TitleEnglish ?? "Unknown Movie",
            MovieTitleAmharic = s.Movie?.TitleAmharic ?? string.Empty,
            MovieGenre = s.Movie?.Genre ?? string.Empty,
            MovieDurationMinutes = s.Movie?.DurationMinutes ?? 0,
            FeaturedImageUrl = s.Movie?.FeaturedImageUrl ?? string.Empty,
            CinemaHallId = s.CinemaHallId,
            HallName = s.CinemaHall?.HallName ?? "Unknown Hall",
            BranchName = s.CinemaHall?.BranchName ?? "Unknown Branch",
            SeatMapMatrixJson = s.CinemaHall?.SeatMapMatrixJson ?? string.Empty,
            Showtime = s.Showtime,
            TicketPrice = s.TicketPrice,
            AvailableSeats = s.AvailableSeats,
            TotalCapacity = s.CinemaHall?.TotalCapacity ?? 0,
            BookedSeats = s.Tickets?.Select(t => t.SeatNumber).ToList() ?? new List<string>()
        };
    }
}
