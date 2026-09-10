using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CineFlow.Application.Common.Interfaces;
using CineFlow.Application.Stars.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CineFlow.Application.Stars.Queries;

public record GetAllStarsQuery : IRequest<List<StarResponseDto>>;

public class GetAllStarsQueryHandler : IRequestHandler<GetAllStarsQuery, List<StarResponseDto>>
{
    private readonly ICineFlowDbContext _context;

    public GetAllStarsQueryHandler(ICineFlowDbContext context)
    {
        _context = context;
    }

    public async Task<List<StarResponseDto>> Handle(GetAllStarsQuery request, CancellationToken cancellationToken)
    {
        var stars = await _context.Stars
            .Include(s => s.Movies)
            .AsNoTracking()
            .OrderBy(s => s.Name)
            .ToListAsync(cancellationToken);

        return stars.Select(s => new StarResponseDto
        {
            Id = s.Id,
            Name = s.Name,
            Bio = s.Bio,
            MovieCount = s.Movies.Count,
            Movies = s.Movies.Select(m => new StarMovieDto
            {
                Id = m.Id,
                TitleEnglish = m.TitleEnglish,
                TitleAmharic = m.TitleAmharic,
                FeaturedImageUrl = m.FeaturedImageUrl,
                Genre = m.Genre
            }).ToList()
        }).ToList();
    }
}
