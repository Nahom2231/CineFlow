using System;

namespace CineFlow.Application.Tickets.Dtos;

public class TicketAdminResponseDto
{
    public Guid Id { get; set; }
    public string SeatNumber { get; set; } = string.Empty;
    public decimal AmountPaid { get; set; }
    public string MockTransactionReference { get; set; } = string.Empty;
    public DateTime PurchasedAt { get; set; }
    public bool IsUsed { get; set; }
    public DateTime? UsedAt { get; set; }
    public Guid ScheduleId { get; set; }
    public string MovieTitle { get; set; } = string.Empty;
    public string HallName { get; set; } = string.Empty;
    public string BranchName { get; set; } = string.Empty;
    public DateTime Showtime { get; set; }
    public string UserId { get; set; } = string.Empty;
}
