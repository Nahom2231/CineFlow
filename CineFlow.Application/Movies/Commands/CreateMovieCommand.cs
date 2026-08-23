using CineFlow.Application.Common.Interfaces;
using CineFlow.Domain.Entities;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MediatR;
using System.IO;

namespace CineFlow.Application.Movies.Commands;

public record CreateMovieCommand(
    string TitleEnglish,
    string? TitleAmharic,
    string DescriptionEnglish,
    string? DescriptionAmharic,
    int DurationMinutes,
    string Genre,
    string AudioLanguage,
    Guid? DirectorId,
    List<Guid>? StarIds,
    String? FeaturedImage,
    List<Stream>? GalleryImages
    ): IRequest<Guid> ;

public class CreateMovieCommandHandler : IRequestHandler<CreateMovieCommand, Guid>
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
        string featuredUrl = request.FeaturedImage ?? string.Empty;

    // 2. Handle GalleryImages
    var galleryUrls = new List<string>();
    if (request.GalleryImages != null && request.GalleryImages.Count > 0)
    {
        foreach (var fileStream in request.GalleryImages)
        {
            if (fileStream != null)
            {
                var url = await _fileStorage.SaveFileAsync(
                    fileStream, 
                    "movies/gallery", 
                    Guid.NewGuid().ToString(), 
                    cancellationToken
                );

                if (!string.IsNullOrEmpty(url))
                {
                    galleryUrls.Add(url);
                }
            }
        }
    }
        var starIds= request.StarIds?? new List<Guid>();
        var stars = await _context.Stars
        .Where(s => starIds.Contains(s.Id))
        .ToListAsync(cancellationToken);

        var movie = new Movie
        {
            Id = Guid.NewGuid(),
            TitleEnglish = request.TitleEnglish,
            TitleAmharic = string.IsNullOrWhiteSpace(request.TitleAmharic) 
              ? request.TitleEnglish 
              : request.TitleAmharic,
              DescriptionEnglish = request.DescriptionEnglish,
              DescriptionAmharic = string.IsNullOrWhiteSpace(request.DescriptionAmharic) 
            ? request.DescriptionEnglish 
            : request.DescriptionAmharic,
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

