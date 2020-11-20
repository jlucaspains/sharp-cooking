using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace SharpCooking.Services
{
    public interface IFileHelper
    {
        Task<bool> ExistsAsync(string filename);
        Task<IEnumerable<string>> GetAllAsync();
        Task WriteTextAsync(string filename, string text);
        Task WriteStreamAsync(string filename, Stream stream);
        Stream ReadStream(string filename);
        Task CopyAsync(string sourceFilePath, string destinationFileName);
        Task MoveAsync(string sourceFilePath, string destinationFileName);
        Task<string> ReadTextAsync(string filename);
        Task DeleteAsync(string filename);
        string GetDocsFolder();
        string GetFilePath(string filename);
    }
}
