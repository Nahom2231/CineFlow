using CineFlow.Application.Common.Interfaces;
using CineFlow.Application.Tickets.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CineFlow.Application.Tickets.Queries;

/// <summary>
/// Query to get available seats for a specific movie showing (schedule).
/// This powers the interactive seat map UI where customers pick their seats.
/// </summary>
public record GetAvailableSeatsQuery(Guid ScheduleId) : IRequest<SeatsMapResponseDto>;

public class GetAvailableSeatsQueryHandler : IRequestHandler<GetAvailableSeatsQuery, SeatsMapResponseDto>
{
    private readonly ICineFlowDbContext _context;

    public GetAvailableSeatsQueryHandler(ICineFlowDbContext context)
    {
        _context = context;
    }

    public async Task<SeatsMapResponseDto> Handle(GetAvailableSeatsQuery request, CancellationToken cancellationToken)
    {
        var schedule = await _context.Schedules
            .Include(s => s.Movie)
            .Include(s => s.CinemaHall)
            .Include(s => s.Tickets)
            .FirstOrDefaultAsync(s => s.Id == request.ScheduleId, cancellationToken);

        if (schedule == null)
        {
            throw new Exception("Schedule not found");
        }

        if (schedule.CinemaHall == null)
        {
            throw new Exception("Cinema hall not found for this schedule");
        }

        if (schedule.Movie == null)
        {
            throw new Exception("Movie not found for this schedule");
        }

        // Parse the seat map from JSON to get all available seats
        var allSeats = GenerateSeatList(schedule.CinemaHall.SeatMapMatrixJson, schedule.CinemaHall.TotalCapacity);
        
        // Get booked seat numbers
        var bookedSeats = schedule.Tickets.Select(t => t.SeatNumber.Trim().ToUpper()).ToHashSet();

        // Map seats with their availability status
        var seatDtos = allSeats.Select(seat => new AvailableSeatDto
        {
            SeatNumber = seat,
            IsAvailable = !bookedSeats.Contains(seat.Trim().ToUpper())
        }).ToList();

        return new SeatsMapResponseDto
        {
            ScheduleId = schedule.Id,
            MovieId = schedule.MovieId,
            MovieTitle = schedule.Movie.TitleEnglish,
            Showtime = schedule.Showtime,
            TicketPrice = schedule.TicketPrice,
            TotalSeats = schedule.CinemaHall.TotalCapacity,
            AvailableSeats = schedule.AvailableSeats,
            Seats = seatDtos
        };
    }

    /// <summary>
    /// Generates seat list from cinema hall configuration.
    /// If SeatMapMatrixJson is provided, parse it; otherwise generate standard seat numbers.
    /// </summary>
    private List<string> GenerateSeatList(string seatMapJson, int totalCapacity)
    {
        var seats = new List<string>();

        // If custom seat map is provided, try to parse it
        if (!string.IsNullOrWhiteSpace(seatMapJson))
        {
            try
            {
                // Assuming format like "A1-A20,B1-B20,C1-C20" or similar
                // This is a simple implementation - adjust based on your seat map format
                var rows = seatMapJson.Split(',');
                foreach (var row in rows)
                {
                    seats.AddRange(row.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
                }

                if (seats.Count > 0)
                    return seats;
            }
            catch
            {
                // If parsing fails, fall back to auto-generation
            }
        }

        // Default: Generate standard seat numbers (A1, A2... B1, B2... etc)
        int seatsAdded = 0;
        for (char row = 'A'; seatsAdded < totalCapacity; row++)
        {
            for (int seatNum = 1; seatNum <= 20 && seatsAdded < totalCapacity; seatNum++)
            {
                seats.Add($"{row}{seatNum}");
                seatsAdded++;
            }
        }

        return seats;
    }
}
