using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CineFlow.Application.Admin.Dtos;
using CineFlow.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CineFlow.Application.Admin.Queries;

public record GetAdminDashboardStatsQuery : IRequest<AdminDashboardStatsDto>;

public class GetAdminDashboardStatsQueryHandler : IRequestHandler<GetAdminDashboardStatsQuery, AdminDashboardStatsDto>
{
    private readonly ICineFlowDbContext _context;

    public GetAdminDashboardStatsQueryHandler(ICineFlowDbContext context)
    {
        _context = context;
    }

    public async Task<AdminDashboardStatsDto> Handle(GetAdminDashboardStatsQuery request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var startOfToday = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0, DateTimeKind.Utc);

        var tickets = await _context.Tickets
            .Include(t => t.Schedule)
                .ThenInclude(s => s!.Movie)
            .Include(t => t.Schedule)
                .ThenInclude(s => s!.CinemaHall)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var totalRevenue = tickets.Sum(t => t.AmountPaid);
        var totalTickets = tickets.Count;

        var todayTickets = tickets.Where(t => t.PurchasedAt >= startOfToday).ToList();
        var todayRevenue = todayTickets.Sum(t => t.AmountPaid);
        var todayTicketsCount = todayTickets.Count;

        var totalActiveMovies = await _context.Movies.CountAsync(cancellationToken);
        var totalUpcomingSchedules = await _context.Schedules.CountAsync(s => s.Showtime >= now, cancellationToken);
        var totalHalls = await _context.CinemaHalls.CountAsync(cancellationToken);

        var recentBookings = tickets
            .OrderByDescending(t => t.PurchasedAt)
            .Take(10)
            .Select(t => new RecentBookingDto
            {
                TicketId = t.Id,
                MovieTitle = t.Schedule?.Movie?.TitleEnglish ?? "Unknown Movie",
                CinemaHall = t.Schedule?.CinemaHall?.HallName ?? "Unknown Hall",
                BranchName = t.Schedule?.CinemaHall?.BranchName ?? "Unknown Branch",
                SeatNumber = t.SeatNumber,
                AmountPaid = t.AmountPaid,
                Showtime = t.Schedule?.Showtime ?? DateTime.MinValue,
                PurchasedAt = t.PurchasedAt,
                IsUsed = t.IsUsed,
                TransactionReference = t.MockTransactionReference,
                CustomerUserId = t.UserId
            })
            .ToList();

        return new AdminDashboardStatsDto
        {
            TotalRevenue = totalRevenue,
            TodayRevenue = todayRevenue,
            TotalTicketsSold = totalTickets,
            TodayTicketsSold = todayTicketsCount,
            TotalActiveMovies = totalActiveMovies,
            TotalUpcomingSchedules = totalUpcomingSchedules,
            TotalCinemaHalls = totalHalls,
            RecentBookings = recentBookings
        };
    }
}
