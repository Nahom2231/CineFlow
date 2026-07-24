using CineFlow.Application.Common.Interfaces;
using CineFlow.Domain.Entities;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace CineFlow.Application.Movies.Commands;

public record CreateMovieCommand(
    string TitleEnglish,
    string TitleAmharic,
    string DescriptionEnglish,
    string DescriptionAmharic,
    int DurationMinutes,
    string Genre,
    string AudioLanguage,
    Guid DirectorId,
    List<Guid> StarIds,
    Stream FeaturedImage,
    List<Stream> GalleryImages);

public class CreateMovieCommandHandler
{
    private readonly ICineFlowDbContext _context;
    private readonly IFileStorageService _fileStorage;

    public CreateMovieCommandHandler(ICineFlowDbContext context, IFileStorageService fileStorage)
    {
        _context = context;
        _fileStorage = fileStorage;

    }

    public async Task<Guid> Handle(CreateMovieCommand request,  CancellationToken cancellationToken)
    {
        var featuredUrl = await _fileStorage.SaveFileAsync(request.FeaturedImage, "movies/thumbnails", Guid.NewGuid().ToString(), cancellationToken);
        var galleryUrls = new List <string> ();

        if(request.GalleryImages != null && request.GalleryImages.Any())
        {
            foreach (var file in request.GalleryImages)
            {
                var url = await _fileStorage.SaveFileAsync(file, "movies/gallery", Guid.NewGuid().ToString(), cancellationToken);
                if(!string.IsNullOrEmpty(url))
                {
                    galleryUrls.Add(url);
                }
            }
        }
        var stars = await _context.Stars
        .Where(s => request.StarIds.Contains(s.Id))
        .ToListAsync(cancellationToken);

        var movie = new Movie
        {
            Id = Guid.NewGuid(),
            TitleEnglish = request.TitleEnglish,
            TitleAmharic = request.TitleAmharic,
            DescriptionEnglish = request.DescriptionEnglish,
            DescriptionAmharic = request.DescriptionAmharic, 
            DurationMinutes = request.DurationMinutes,
            Genre =request.Genre,
            AudioLanguage = request.AudioLanguage,
            FeaturedImageUrl = featuredUrl,
            GalleryImageUrls = galleryUrls,
            DirectorId =request.DirectorId,
            Stars = stars
        };

        _context.Movies.Add(movie);
        await _context.SaveChangesAsync(cancellationToken);

        return movie.Id;
    }
}

