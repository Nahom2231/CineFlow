using CineFlow.Application.Common.Interfaces;
using CineFlow.Application.Movies.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CineFlow.Application.Movies.Queries;

public record GetFilteredMoviesQuery(
    string? SearchTitle,
    string? Genre,
    string ? AudioLanguage,
    string? CinemaBranch) : IRequest<List<MovieDto>>;

public class GetFilteredMoviesQueryHandler : IRequestHandler<GetFilteredMoviesQuery, List<MovieDto>>
{
    private readonly ICineFlowDbContext _context;

    public GetFilteredMoviesQueryHandler(ICineFlowDbContext context)
    {
        _context = context;
    }

    public async Task<List<MovieDto>> Handle(GetFilteredMoviesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Movies
        .Include(m => m.Director)
        .Include(m => m.Stars)
        .Include(m=>m.Schedules)
        .AsNoTracking();

        if(!string.IsNullOrWhiteSpace(request.SearchTitle))
        {
            var search = request.SearchTitle.ToLower();
            query= query.Where(m=>m.TitleEnglish.ToLower().Contains(search)||m.TitleAmharic.Contains(search));
        }
        if(!string.IsNullOrWhiteSpace(request.Genre))
        {
            query = query.Where(m=>m.Genre.Contains(request.Genre));
        }
        if (!string.IsNullOrWhiteSpace(request.AudioLanguage))
        {
            query = query.Where(m=>m.AudioLanguage==request.AudioLanguage);

        }

        if (!string.IsNullOrWhiteSpace(request.CinemaBranch))
        {
            query = query.Where(m=>m.Schedules.Any(s=>s.CinemaHall!= null && s.CinemaHall.BranchName == request.CinemaBranch));
        }
    
        return await query.Select(m => new MovieDto
        {
            Id=m.Id,
            TitleEnglish = m.TitleEnglish,
            TitleAmharic = m.TitleAmharic,
            DescriptionEnglish = m.DescriptionEnglish,
            DescriptionAmharic = m.DescriptionAmharic,
            DurationMinutes = m.DurationMinutes,
            Genre=m.Genre,
            AudioLanguage = m.AudioLanguage,
            FeaturedImageUrl= m.FeaturedImageUrl,
            GalleryImageUrls = m.GalleryImageUrls,
            DirectorName =m.Director != null ? m.Director.Name : string.Empty,
            StarName =m.Stars.Select(s=>s.Name).ToList()
        }).ToListAsync(cancellationToken);

    }

}
