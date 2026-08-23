using CineFlow.Application.Common.Interfaces;
using CineFlow.Application.Tickets.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CineFlow.Application.Tickets.Queries;

/// <summary>
/// Query to get all tickets/bookings for a specific user.
/// </summary>
public record GetUserBookingsQuery(string UserId) : IRequest<List<TicketWithQRResponseDto>>;

public class GetUserBookingsQueryHandler : IRequestHandler<GetUserBookingsQuery, List<TicketWithQRResponseDto>>
{
    private readonly ICineFlowDbContext _context;

    public GetUserBookingsQueryHandler(ICineFlowDbContext context)
    {
        _context = context;
    }

    public async Task<List<TicketWithQRResponseDto>> Handle(GetUserBookingsQuery request, CancellationToken cancellationToken)
    {
        var tickets = await _context.Tickets
            .Include(t => t.Schedule)
                .ThenInclude(s => s!.Movie)
            .Include(t => t.Schedule)
                .ThenInclude(s => s!.CinemaHall)
            .Where(t => t.UserId == request.UserId)
            .OrderByDescending(t => t.PurchasedAt) // Adjust property name if needed
            .ToListAsync(cancellationToken);

        // Map to your DTO structure
        return tickets.Select(t => new TicketWithQRResponseDto
        {
            // Map fields here matching your TicketWithQRResponseDto
        }).ToList();
    }
}