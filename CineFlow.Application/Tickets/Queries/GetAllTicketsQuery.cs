using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CineFlow.Application.Common.Interfaces;
using CineFlow.Application.Tickets.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CineFlow.Application.Tickets.Queries;

public record GetAllTicketsQuery(Guid? ScheduleId = null, string? UserId = null) : IRequest<List<TicketAdminResponseDto>>;

public class GetAllTicketsQueryHandler : IRequestHandler<GetAllTicketsQuery, List<TicketAdminResponseDto>>
{
    private readonly ICineFlowDbContext _context;

    public GetAllTicketsQueryHandler(ICineFlowDbContext context)
    {
        _context = context;
    }

    public async Task<List<TicketAdminResponseDto>> Handle(GetAllTicketsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Tickets
            .Include(t => t.Schedule)
                .ThenInclude(s => s!.Movie)
            .Include(t => t.Schedule)
                .ThenInclude(s => s!.CinemaHall)
            .AsNoTracking()
            .AsQueryable();

        if (request.ScheduleId.HasValue)
        {
            query = query.Where(t => t.ScheduleId == request.ScheduleId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.UserId))
        {
            query = query.Where(t => t.UserId == request.UserId);
        }

        var tickets = await query
            .OrderByDescending(t => t.PurchasedAt)
            .ToListAsync(cancellationToken);

        return tickets.Select(t => new TicketAdminResponseDto
        {
            Id = t.Id,
            SeatNumber = t.SeatNumber,
            AmountPaid = t.AmountPaid,
            MockTransactionReference = t.MockTransactionReference,
            PurchasedAt = t.PurchasedAt,
            IsUsed = t.IsUsed,
            UsedAt = t.UsedAt,
            ScheduleId = t.ScheduleId,
            MovieTitle = t.Schedule?.Movie?.TitleEnglish ?? "Unknown Movie",
            HallName = t.Schedule?.CinemaHall?.HallName ?? "Unknown Hall",
            BranchName = t.Schedule?.CinemaHall?.BranchName ?? "Unknown Branch",
            Showtime = t.Schedule?.Showtime ?? DateTime.MinValue,
            UserId = t.UserId
        }).ToList();
    }
}
