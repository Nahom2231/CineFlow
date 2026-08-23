namespace CineFlow.Application.Tickets.Dtos;

public class TicketWithQRResponseDto
{
    public Guid TicketId { get; set; }
    public string SeatNumber { get; set; } = string.Empty;
    public decimal AmountPaid { get; set; }
    public string MovieTitle { get; set; } = string.Empty;
    public DateTime Showtime { get; set; }
    public string CinemaHall { get; set; } = string.Empty;
    public string BranchName { get; set; } = string.Empty;
    public DateTime PurchasedAt { get; set; }
    public bool IsUsed { get; set; }
    public DateTime? UsedAt { get; set; }
    
    /// <summary>
    /// QR Code data in Base64 format. Can be converted to image on frontend using QR code library.
    /// </summary>
    public string QRCodeData { get; set; } = string.Empty;
    
    /// <summary>
    /// Mock transaction reference for payment tracking
    /// </summary>
    public string TransactionReference { get; set; } = string.Empty;
}
