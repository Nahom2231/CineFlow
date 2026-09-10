using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CineFlow.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CineFlow.Application.Movies.Commands;

public record UpdateMovieCommand(
    Guid Id,
    string TitleEnglish,
    string? TitleAmharic,
    string DescriptionEnglish,
    string? DescriptionAmharic,
    int DurationMinutes,
    string Genre,
    string AudioLanguage,
    Guid? DirectorId,
    List<Guid>? StarIds,
    string? FeaturedImage,
    List<Stream>? GalleryImages,
    List<string>? ExistingGalleryImages
) : IRequest<bool>;

public class UpdateMovieCommandHandler : IRequestHandler<UpdateMovieCommand, bool>
{
    private readonly ICineFlowDbContext _context;
    private readonly IFileStorageService _fileStorage;

    public UpdateMovieCommandHandler(ICineFlowDbContext context, IFileStorageService fileStorage)
    {
        _context = context;
        _fileStorage = fileStorage;
    }

    public async Task<bool> Handle(UpdateMovieCommand request, CancellationToken cancellationToken)
    {
        var movie = await _context.Movies
            .Include(m => m.Stars)
            .FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken);

        if (movie == null) return false;

        movie.TitleEnglish = request.TitleEnglish;
        movie.TitleAmharic = string.IsNullOrWhiteSpace(request.TitleAmharic)
            ? request.TitleEnglish
            : request.TitleAmharic;
        movie.DescriptionEnglish = request.DescriptionEnglish;
        movie.DescriptionAmharic = string.IsNullOrWhiteSpace(request.DescriptionAmharic)
            ? request.DescriptionEnglish
            : request.DescriptionAmharic;
        movie.DurationMinutes = request.DurationMinutes;
        movie.Genre = request.Genre;
        movie.AudioLanguage = request.AudioLanguage;
        movie.DirectorId = request.DirectorId;

        if (!string.IsNullOrWhiteSpace(request.FeaturedImage))
        {
            movie.FeaturedImageUrl = request.FeaturedImage;
        }

        var galleryUrls = request.ExistingGalleryImages != null
            ? new List<string>(request.ExistingGalleryImages)
            : new List<string>(movie.GalleryImageUrls ?? new List<string>());

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
        movie.GalleryImageUrls = galleryUrls;

        if (request.StarIds != null)
        {
            var stars = await _context.Stars
                .Where(s => request.StarIds.Contains(s.Id))
                .ToListAsync(cancellationToken);

            movie.Stars.Clear();
            foreach (var s in stars)
            {
                movie.Stars.Add(s);
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
