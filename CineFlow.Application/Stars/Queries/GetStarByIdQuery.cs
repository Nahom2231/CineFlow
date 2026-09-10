using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CineFlow.Application.Common.Interfaces;
using CineFlow.Application.Stars.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CineFlow.Application.Stars.Queries;

public record GetStarByIdQuery(Guid Id) : IRequest<StarResponseDto?>;

public class GetStarByIdQueryHandler : IRequestHandler<GetStarByIdQuery, StarResponseDto?>
{
    private readonly ICineFlowDbContext _context;

    public GetStarByIdQueryHandler(ICineFlowDbContext context)
    {
        _context = context;
    }

    public async Task<StarResponseDto?> Handle(GetStarByIdQuery request, CancellationToken cancellationToken)
    {
        var star = await _context.Stars
            .Include(s => s.Movies)
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        if (star == null) return null;

        return new StarResponseDto
        {
            Id = star.Id,
            Name = star.Name,
            Bio = star.Bio,
            MovieCount = star.Movies.Count,
            Movies = star.Movies.Select(m => new StarMovieDto
            {
                Id = m.Id,
                TitleEnglish = m.TitleEnglish,
                TitleAmharic = m.TitleAmharic,
                FeaturedImageUrl = m.FeaturedImageUrl,
                Genre = m.Genre
            }).ToList()
        };
    }
}
