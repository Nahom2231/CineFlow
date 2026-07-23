using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace CineFlow.Application.Common.Interfaces;

public interface IFileStorageService
{
    Task<string> SaveFileAsync(Stream fileStream, string fileName, string folderName, CancellationToken cancellationToken);
}