using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CineFlow.Application.Admin.Dtos;
using CineFlow.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CineFlow.Application.Admin.Queries;

public record GetDailyRevenueQuery(int Days = 7) : IRequest<List<DailyRevenueDto>>;

public class GetDailyRevenueQueryHandler : IRequestHandler<GetDailyRevenueQuery, List<DailyRevenueDto>>
{
    private readonly ICineFlowDbContext _context;

    public GetDailyRevenueQueryHandler(ICineFlowDbContext context)
    {
        _context = context;
    }

    public async Task<List<DailyRevenueDto>> Handle(GetDailyRevenueQuery request, CancellationToken cancellationToken)
    {
        var daysToQuery = Math.Clamp(request.Days, 1, 90);
        var startDate = DateTime.UtcNow.Date.AddDays(-(daysToQuery - 1));

        var tickets = await _context.Tickets
            .Where(t => t.PurchasedAt >= startDate)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var list = new List<DailyRevenueDto>();

        for (int i = 0; i < daysToQuery; i++)
        {
            var targetDay = startDate.AddDays(i);
            var nextDay = targetDay.AddDays(1);

            var dayTickets = tickets.Where(t => t.PurchasedAt >= targetDay && t.PurchasedAt < nextDay).ToList();

            list.Add(new DailyRevenueDto
            {
                Date = targetDay.ToString("yyyy-MM-dd"),
                Revenue = dayTickets.Sum(t => t.AmountPaid),
                TicketsSold = dayTickets.Count
            });
        }

        return list;
    }
}
