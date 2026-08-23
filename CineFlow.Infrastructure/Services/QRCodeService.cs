using System.Text;
using System.Text.Json;
using CineFlow.Application.Common.Interfaces;

namespace CineFlow.Infrastructure.Services;

/// <summary>
/// Implementation of QR code service for generating digital tickets.
/// </summary>
public class QRCodeService : IQRCodeService
{
    public string GenerateQRCodeData(Guid ticketId, string seatNumber, Guid scheduleId, string movieTitle)
    {
        // Create a structured QR code data containing all necessary ticket information
        var qrData = new
        {
            TicketId = ticketId.ToString(),
            SeatNumber = seatNumber,
            ScheduleId = scheduleId.ToString(),
            MovieTitle = movieTitle,
            GeneratedAt = DateTime.UtcNow,
            CheckSum = GenerateChecksum(ticketId, seatNumber)
        };

        // Serialize to JSON format
        var jsonData = JsonSerializer.Serialize(qrData);
        
        // Encode to Base64 for compact QR representation
        var base64Data = Convert.ToBase64String(Encoding.UTF8.GetBytes(jsonData));
        
        return base64Data;
    }

    private string GenerateChecksum(Guid ticketId, string seatNumber)
    {
        // Simple checksum to prevent tampering
        var combined = ticketId.ToString() + seatNumber;
        using (var sha256 = System.Security.Cryptography.SHA256.Create())
        {
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(combined));
            return Convert.ToHexString(hashedBytes)[..16]; // First 16 characters
        }
    }
}
