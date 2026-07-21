namespace CineFlow.Domain.Entities;
public class Ticket
{
    public Guid Id { get; set; }
    public string SeatNumber { get; set; } = string.Empty;
    public decimal AmountPaid {get; set;}
    public string MockTransactionReference { get; set; } = string.Empty;
    
    public DateTime PurchasedAt { get; set; } = DateTime.UtcNow;

    public Guid ScheduleId { get; set; }
    public Schedule? Schedule { get; set; } 
    
    public Guid UserId { get; set; }
}