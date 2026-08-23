namespace CineFlow.Application.Tickets.Dtos;

public class AvailableSeatDto
{
    public string SeatNumber { get; set; } = string.Empty;
    public bool IsAvailable { get; set; }
}

public class SeatsMapResponseDto
{
    public Guid ScheduleId { get; set; }
    public Guid MovieId { get; set; }
    public string MovieTitle { get; set; } = string.Empty;
    public DateTime Showtime { get; set; }
    public decimal TicketPrice { get; set; }
    public int TotalSeats { get; set; }
    public int AvailableSeats { get; set; }
    public List<AvailableSeatDto> Seats { get; set; } = new();
}
