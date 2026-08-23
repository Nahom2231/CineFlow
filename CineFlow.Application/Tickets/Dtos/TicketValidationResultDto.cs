namespace CineFlow.Application.Tickets.Dtos;

public class TicketValidationResultDto
{
    public bool IsValid { get; set; }
    public string Status { get; set; } = string.Empty; // "VALID", "ALREADY_USED", "INVALID_TICKET", "EXPIRED"
    public string Message { get; set; } = string.Empty;
    public DateTime? ScannedAt { get; set; }

    // Enriched Ticket Details for door staff visual confirmation
    public Guid? TicketId { get; set; }
    public string? MovieTitle { get; set; }
    public string? CinemaHall { get; set; }
    public string? CinemaBranch { get; set; }
    public string? SeatNumber { get; set; }
    public decimal? AmountPaid { get; set; }
    public DateTime? Showtime { get; set; }
    public string? CustomerUserId { get; set; }
    public string? TransactionReference { get; set; }
    public DateTime? PreviousUsedAt { get; set; }
}
