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

public record GetRevenueByMovieQuery : IRequest<List<MovieRevenueDto>>;

public class GetRevenueByMovieQueryHandler : IRequestHandler<GetRevenueByMovieQuery, List<MovieRevenueDto>>
{
    private readonly ICineFlowDbContext _context;

    public GetRevenueByMovieQueryHandler(ICineFlowDbContext context)
    {
        _context = context;
    }

    public async Task<List<MovieRevenueDto>> Handle(GetRevenueByMovieQuery request, CancellationToken cancellationToken)
    {
        var movies = await _context.Movies
            .Include(m => m.Schedules)
                .ThenInclude(s => s.Tickets)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var totalSystemRevenue = movies
            .SelectMany(m => m.Schedules)
            .SelectMany(s => s.Tickets)
            .Sum(t => t.AmountPaid);

        var results = movies.Select(m =>
        {
            var tickets = m.Schedules.SelectMany(s => s.Tickets).ToList();
            var movieRevenue = tickets.Sum(t => t.AmountPaid);
            var percentage = totalSystemRevenue > 0
                ? (double)(movieRevenue / totalSystemRevenue) * 100
                : 0;

            return new MovieRevenueDto
            {
                MovieId = m.Id,
                TitleEnglish = m.TitleEnglish,
                TitleAmharic = m.TitleAmharic,
                Genre = m.Genre,
                FeaturedImageUrl = m.FeaturedImageUrl,
                TicketsSold = tickets.Count,
                TotalRevenue = movieRevenue,
                TotalSchedulesCount = m.Schedules.Count,
                RevenuePercentage = Math.Round(percentage, 2)
            };
        })
        .OrderByDescending(r => r.TotalRevenue)
        .ToList();

        return results;
    }
}
