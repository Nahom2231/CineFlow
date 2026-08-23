namespace CineFlow.Application.Common.Interfaces;

/// <summary>
/// Service to generate QR codes for digital tickets.
/// </summary>
public interface IQRCodeService
{
    /// <summary>
    /// Generates a QR code string representing ticket information.
    /// </summary>
    string GenerateQRCodeData(Guid ticketId, string seatNumber, Guid scheduleId, string movieTitle);
}
