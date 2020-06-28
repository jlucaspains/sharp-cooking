using System.Threading.Tasks;

namespace SharpCooking.Services
{
    public interface IBackupProvider
    {
        bool IsAuthorized { get; }
        string ProviderName { get; }
        Task<bool> BackupFile(string localFilePath);
        Task Authorize();
    }
}
