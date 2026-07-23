using CineFlow.Application.Common.Interfaces;

using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace CineFlow.Infrastructure.Services;

public class LocalFileStorageService : IFileStorageService
{
    // Changed to use Stream and filename to avoid depending on Microsoft.AspNetCore.Http
    public async Task<string> SaveFileAsync(Stream fileStream, string fileName, string folderName, CancellationToken cancellationToken)
    {
        if (fileStream == null || (fileStream.CanSeek && fileStream.Length == 0))
            return string.Empty;

        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", folderName);

        if (!Directory.Exists(uploadsFolder))
        {
            Directory.CreateDirectory(uploadsFolder);
        }

        var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(fileName)}";
        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

        // Ensure stream position is at beginning if possible
        if (fileStream.CanSeek)
            fileStream.Position = 0;

        using (var destination = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            await fileStream.CopyToAsync(destination, 81920, cancellationToken);
        }

        return $"/uploads/{folderName}/{uniqueFileName}";
    }
}