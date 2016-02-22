using Microsoft.WindowsAzure.MobileServices.Files;
using Microsoft.WindowsAzure.MobileServices.Files.Metadata;
using Microsoft.WindowsAzure.MobileServices.Sync;
using System.Threading.Tasks;

namespace ContosoMoments
{
    public interface IPlatform
    {
        Task<string> GetDataFilesPath();

        Task<IMobileServiceFileDataSource> GetFileDataSource(MobileServiceFileMetadata metadata);

        Task DownloadFileAsync<T>(IMobileServiceSyncTable<T> table, MobileServiceFile file, string fullPath);

        string GetDataPathAsync();

        Task<string> TakePhotoAsync(object context);
    }
}
