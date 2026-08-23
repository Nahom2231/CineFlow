using System;
using System.Text;
using System.Text.Json;
using CineFlow.Application.Common.Interfaces;
using CineFlow.Application.Tickets.Dtos;
using CineFlow.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CineFlow.Application.Tickets.Commands;

public record ValidateTicketCommand(string CodeOrReference) : IRequest<TicketValidationResultDto>;

public class ValidateTicketCommandHandler : IRequestHandler<ValidateTicketCommand, TicketValidationResultDto>
{
    private readonly ICineFlowDbContext _context;

    public ValidateTicketCommandHandler(ICineFlowDbContext context)
    {
        _context = context;
    }

    public async Task<TicketValidationResultDto> Handle(ValidateTicketCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.CodeOrReference))
        {
            return new TicketValidationResultDto
            {
                IsValid = false,
                Status = "INVALID_INPUT",
                Message = "No QR code or ticket reference was provided."
            };
        }

        var input = request.CodeOrReference.Trim();
        Guid? extractedTicketId = null;
        string? extractedTxnRef = null;

        // 1. Try parsing direct Guid
        if (Guid.TryParse(input, out var directGuid))
        {
            extractedTicketId = directGuid;
        }
        else
        {
            // 2. Try parsing Base64 QR Code payload
            try
            {
                var decodedBytes = Convert.FromBase64String(input);
                var decodedJson = Encoding.UTF8.GetString(decodedBytes);
                using var jsonDoc = JsonDocument.Parse(decodedJson);
                if (jsonDoc.RootElement.TryGetProperty("TicketId", out var ticketIdProp) &&
                    Guid.TryParse(ticketIdProp.GetString(), out var parsedTicketGuid))
                {
                    extractedTicketId = parsedTicketGuid;
                }
            }
            catch
            {
                // Not a valid base64 JSON payload, treat as raw transaction reference
                extractedTxnRef = input;
            }
        }

        // Query database
        IQueryable<Ticket> query = _context.Tickets
            .Include(t => t.Schedule)
                .ThenInclude(s => s!.Movie)
            .Include(t => t.Schedule)
                .ThenInclude(s => s!.CinemaHall);

        Ticket? ticket = null;
        if (extractedTicketId.HasValue)
        {
            ticket = await query.FirstOrDefaultAsync(t => t.Id == extractedTicketId.Value, cancellationToken);
        }
        else
        {
            ticket = await query.FirstOrDefaultAsync(t => t.MockTransactionReference == input || t.MockTransactionReference == extractedTxnRef, cancellationToken);
        }

        if (ticket == null)
        {
            return new TicketValidationResultDto
            {
                IsValid = false,
                Status = "NOT_FOUND",
                Message = "Fake or unrecognized ticket! No matching booking found in system."
            };
        }

        var movieTitle = ticket.Schedule?.Movie?.TitleEnglish ?? "Unknown Movie";
        var hallName = ticket.Schedule?.CinemaHall?.HallName ?? "Unknown Hall";
        var branchName = ticket.Schedule?.CinemaHall?.BranchName ?? "Unknown Branch";
        var showtime = ticket.Schedule?.Showtime ?? DateTime.MinValue;

        // Check if ticket was already scanned / used (Anti-Fraud protection)
        if (ticket.IsUsed)
        {
            return new TicketValidationResultDto
            {
                IsValid = false,
                Status = "ALREADY_USED",
                Message = $"FRAUD ALERT: This ticket was already used on {ticket.UsedAt:yyyy-MM-dd HH:mm:ss} UTC! Entry denied.",
                ScannedAt = DateTime.UtcNow,
                PreviousUsedAt = ticket.UsedAt,
                TicketId = ticket.Id,
                MovieTitle = movieTitle,
                CinemaHall = hallName,
                CinemaBranch = branchName,
                SeatNumber = ticket.SeatNumber,
                AmountPaid = ticket.AmountPaid,
                Showtime = showtime,
                CustomerUserId = ticket.UserId,
                TransactionReference = ticket.MockTransactionReference
            };
        }

        // Mark ticket as used atomically
        ticket.IsUsed = true;
        ticket.UsedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        return new TicketValidationResultDto
        {
            IsValid = true,
            Status = "VALID",
            Message = $"Ticket Verified! Welcome to {movieTitle}. Seat: {ticket.SeatNumber} ({hallName}).",
            ScannedAt = ticket.UsedAt,
            TicketId = ticket.Id,
            MovieTitle = movieTitle,
            CinemaHall = hallName,
            CinemaBranch = branchName,
            SeatNumber = ticket.SeatNumber,
            AmountPaid = ticket.AmountPaid,
            Showtime = showtime,
            CustomerUserId = ticket.UserId,
            TransactionReference = ticket.MockTransactionReference
        };
    }
}