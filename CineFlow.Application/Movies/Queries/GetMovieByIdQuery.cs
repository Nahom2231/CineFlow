using CineFlow.Application.Common.Interfaces;
using CineFlow.Application.Movies.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CineFlow.Application.Movies.Queries;

public record GetMovieByIdQuery(Guid Id) : IRequest<MovieResponseDto?>;

public class GetMovieByIdQueryHandler : IRequestHandler<GetMovieByIdQuery, MovieResponseDto?>
{
    private readonly ICineFlowDbContext _context;

    public GetMovieByIdQueryHandler(ICineFlowDbContext context)
    {
        _context = context;
    }

    public async Task<MovieResponseDto?> Handle(GetMovieByIdQuery request, CancellationToken cancellationToken)
    {
        var m = await _context.Movies
            .Include(m => m.Director)
            .Include(m => m.Stars)
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken);

        if (m == null) return null;

        return new MovieResponseDto
        {
            Id = m.Id,
            TitleEnglish = m.TitleEnglish,
            TitleAmharic = m.TitleAmharic,
            DescriptionEnglish = m.DescriptionEnglish,
            DescriptionAmharic = m.DescriptionAmharic,
            DurationMinutes = m.DurationMinutes,
            Genre = m.Genre,
            AudioLanguage = m.AudioLanguage,
            FeaturedImageUrl = m.FeaturedImageUrl,
            GalleryImageUrls = m.GalleryImageUrls,
            DirectorName = m.Director != null ? m.Director.Name : string.Empty,
            StarName = m.Stars.Select(s => s.Name).ToList()
        };
    }
}
