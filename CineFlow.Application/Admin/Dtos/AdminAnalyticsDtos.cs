using System;
using System.Collections.Generic;

namespace CineFlow.Application.Admin.Dtos;

public class AdminDashboardStatsDto
{
    public decimal TotalRevenue { get; set; }
    public decimal TodayRevenue { get; set; }
    public int TotalTicketsSold { get; set; }
    public int TodayTicketsSold { get; set; }
    public int TotalActiveMovies { get; set; }
    public int TotalUpcomingSchedules { get; set; }
    public int TotalCinemaHalls { get; set; }
    public List<RecentBookingDto> RecentBookings { get; set; } = new();
}

public class RecentBookingDto
{
    public Guid TicketId { get; set; }
    public string MovieTitle { get; set; } = string.Empty;
    public string CinemaHall { get; set; } = string.Empty;
    public string BranchName { get; set; } = string.Empty;
    public string SeatNumber { get; set; } = string.Empty;
    public decimal AmountPaid { get; set; }
    public DateTime Showtime { get; set; }
    public DateTime PurchasedAt { get; set; }
    public bool IsUsed { get; set; }
    public string TransactionReference { get; set; } = string.Empty;
    public string CustomerUserId { get; set; } = string.Empty;
}

public class MovieRevenueDto
{
    public Guid MovieId { get; set; }
    public string TitleEnglish { get; set; } = string.Empty;
    public string TitleAmharic { get; set; } = string.Empty;
    public string Genre { get; set; } = string.Empty;
    public string FeaturedImageUrl { get; set; } = string.Empty;
    public int TicketsSold { get; set; }
    public decimal TotalRevenue { get; set; }
    public int TotalSchedulesCount { get; set; }
    public double RevenuePercentage { get; set; }
}

public class DailyRevenueDto
{
    public string Date { get; set; } = string.Empty; // Format: "YYYY-MM-DD"
    public decimal Revenue { get; set; }
    public int TicketsSold { get; set; }
}
