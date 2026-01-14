using System.IO;
using System.Threading.Tasks;

namespace EventsMS.Application.Interfaces
{
    public interface IFileStorageService
    {
        Task<string> SaveFileAsync(string containerName, Stream fileStream, string fileName);
        Task DeleteFileAsync(string route, string containerName);
    }
}
