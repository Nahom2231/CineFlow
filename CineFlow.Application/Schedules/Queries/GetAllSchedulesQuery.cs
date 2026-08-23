using CineFlow.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CineFlow.Application.Schedules.Queries;

public class ScheduleDto
{
    public Guid Id { get; set; }
    public Guid MovieId { get; set; }
    public string MovieTitle { get; set; } = string.Empty;
    public string MovieGenre { get; set; } = string.Empty;
    public int MovieDurationMinutes { get; set; }
    public string FeaturedImageUrl { get; set; } = string.Empty;
    public Guid CinemaHallId { get; set; }
    public string HallName { get; set; } = string.Empty;
    public string BranchName { get; set; } = string.Empty;
    public DateTime Showtime { get; set; }
    public decimal TicketPrice { get; set; }
    public int AvailableSeats { get; set; }
    public int TotalCapacity { get; set; }
    public int BookedSeatsCount { get; set; }
}

public record GetAllSchedulesQuery(Guid? MovieId = null, string? CinemaBranch = null) : IRequest<List<ScheduleDto>>;

public class GetAllSchedulesQueryHandler : IRequestHandler<GetAllSchedulesQuery, List<ScheduleDto>>
{
    private readonly ICineFlowDbContext _context;

    public GetAllSchedulesQueryHandler(ICineFlowDbContext context)
    {
        _context = context;
    }

    public async Task<List<ScheduleDto>> Handle(GetAllSchedulesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Schedules
            .Include(s => s.Movie)
            .Include(s => s.CinemaHall)
            .Include(s => s.Tickets)
            .AsNoTracking();

        if (request.MovieId.HasValue && request.MovieId.Value != Guid.Empty)
        {
            query = query.Where(s => s.MovieId == request.MovieId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.CinemaBranch))
        {
            query = query.Where(s => s.CinemaHall != null && s.CinemaHall.BranchName.ToLower() == request.CinemaBranch.ToLower());
        }

        var schedules = await query
            .OrderBy(s => s.Showtime)
            .ToListAsync(cancellationToken);

        return schedules.Select(s => new ScheduleDto
        {
            Id = s.Id,
            MovieId = s.MovieId,
            MovieTitle = s.Movie?.TitleEnglish ?? "Unknown Movie",
            MovieGenre = s.Movie?.Genre ?? string.Empty,
            MovieDurationMinutes = s.Movie?.DurationMinutes ?? 0,
            FeaturedImageUrl = s.Movie?.FeaturedImageUrl ?? string.Empty,
            CinemaHallId = s.CinemaHallId,
            HallName = s.CinemaHall?.HallName ?? "Unknown Hall",
            BranchName = s.CinemaHall?.BranchName ?? "Unknown Branch",
            Showtime = s.Showtime,
            TicketPrice = s.TicketPrice,
            AvailableSeats = s.AvailableSeats,
            TotalCapacity = s.CinemaHall?.TotalCapacity ?? 0,
            BookedSeatsCount = s.Tickets?.Count ?? 0
        }).ToList();
    }
}
