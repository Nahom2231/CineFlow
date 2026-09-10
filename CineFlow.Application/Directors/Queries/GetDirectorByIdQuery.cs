using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CineFlow.Application.Common.Interfaces;
using CineFlow.Application.Directors.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CineFlow.Application.Directors.Queries;

public record GetDirectorByIdQuery(Guid Id) : IRequest<DirectorResponseDto?>;

public class GetDirectorByIdQueryHandler : IRequestHandler<GetDirectorByIdQuery, DirectorResponseDto?>
{
    private readonly ICineFlowDbContext _context;

    public GetDirectorByIdQueryHandler(ICineFlowDbContext context)
    {
        _context = context;
    }

    public async Task<DirectorResponseDto?> Handle(GetDirectorByIdQuery request, CancellationToken cancellationToken)
    {
        var director = await _context.Directors
            .Include(d => d.Movies)
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken);

        if (director == null) return null;

        return new DirectorResponseDto
        {
            Id = director.Id,
            Name = director.Name,
            Bio = director.Bio,
            MovieCount = director.Movies.Count,
            Movies = director.Movies.Select(m => new DirectorMovieDto
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
