using CineFlow.Application.Common.Interfaces;
using CineFlow.Application.Tickets.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CineFlow.Application.Tickets.Queries;

/// <summary>
/// Query to get a ticket with its QR code.
/// Customers use this to view their digital pass with QR code for entry.
/// </summary>
public record GetTicketWithQRQuery(Guid TicketId, string UserId) : IRequest<TicketWithQRResponseDto>;

public class GetTicketWithQRQueryHandler : IRequestHandler<GetTicketWithQRQuery, TicketWithQRResponseDto>
{
    private readonly ICineFlowDbContext _context;
    private readonly IQRCodeService _qrCodeService;

    public GetTicketWithQRQueryHandler(ICineFlowDbContext context, IQRCodeService qrCodeService)
    {
        _context = context;
        _qrCodeService = qrCodeService;
    }

    public async Task<TicketWithQRResponseDto> Handle(GetTicketWithQRQuery request, CancellationToken cancellationToken)
    {
        var ticket = await _context.Tickets
            .Include(t => t.Schedule)
            .ThenInclude(s => s!.Movie)
            .Include(t => t.Schedule)
            .ThenInclude(s => s!.CinemaHall)
            .FirstOrDefaultAsync(t => t.Id == request.TicketId && t.UserId == request.UserId, cancellationToken);

        if (ticket == null)
        {
            throw new Exception("Ticket not found or you don't have permission to access it");
        }

        if (ticket.Schedule == null || ticket.Schedule.Movie == null || ticket.Schedule.CinemaHall == null)
        {
            throw new Exception("Ticket information is incomplete");
        }

        // Generate QR code data
        var qrCodeData = _qrCodeService.GenerateQRCodeData(
            ticket.Id,
            ticket.SeatNumber,
            ticket.ScheduleId,
            ticket.Schedule.Movie.TitleEnglish
        );

        return new TicketWithQRResponseDto
        {
            TicketId = ticket.Id,
            SeatNumber = ticket.SeatNumber,
            AmountPaid = ticket.AmountPaid,
            MovieTitle = ticket.Schedule.Movie.TitleEnglish,
            Showtime = ticket.Schedule.Showtime,
            CinemaHall = ticket.Schedule.CinemaHall.HallName,
            BranchName = ticket.Schedule.CinemaHall.BranchName,
            PurchasedAt = ticket.PurchasedAt,
            IsUsed = ticket.IsUsed,
            UsedAt = ticket.UsedAt,
            QRCodeData = qrCodeData,
            TransactionReference = ticket.MockTransactionReference
        };
    }
}
