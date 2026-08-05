namespace CineFlow.Domain.Entities;
public class Ticket
{
    public Guid Id { get; set; }
    public string SeatNumber { get; set; } = string.Empty;
    public decimal AmountPaid {get; set;}
    public string MockTransactionReference { get; set; } = string.Empty;
    
    public DateTime PurchasedAt { get; set; } = DateTime.UtcNow;

    public bool IsUsed { get; set; } = false;
    public DateTime? UsedAt { get; set; }

    public Guid ScheduleId { get; set; }
    public Schedule? Schedule { get; set; } 
    
    public string UserId { get; set; }= string.Empty;
}