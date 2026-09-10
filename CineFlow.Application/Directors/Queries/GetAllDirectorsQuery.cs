using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CineFlow.Application.Common.Interfaces;
using CineFlow.Application.Directors.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CineFlow.Application.Directors.Queries;

public record GetAllDirectorsQuery : IRequest<List<DirectorResponseDto>>;

public class GetAllDirectorsQueryHandler : IRequestHandler<GetAllDirectorsQuery, List<DirectorResponseDto>>
{
    private readonly ICineFlowDbContext _context;

    public GetAllDirectorsQueryHandler(ICineFlowDbContext context)
    {
        _context = context;
    }

    public async Task<List<DirectorResponseDto>> Handle(GetAllDirectorsQuery request, CancellationToken cancellationToken)
    {
        var directors = await _context.Directors
            .Include(d => d.Movies)
            .AsNoTracking()
            .OrderBy(d => d.Name)
            .ToListAsync(cancellationToken);

        return directors.Select(d => new DirectorResponseDto
        {
            Id = d.Id,
            Name = d.Name,
            Bio = d.Bio,
            MovieCount = d.Movies.Count,
            Movies = d.Movies.Select(m => new DirectorMovieDto
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
