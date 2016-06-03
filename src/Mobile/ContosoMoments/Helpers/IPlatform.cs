using Microsoft.WindowsAzure.MobileServices.Files;
using Microsoft.WindowsAzure.MobileServices.Files.Metadata;
using Microsoft.WindowsAzure.MobileServices.Sync;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices;
using Newtonsoft.Json.Linq;
using Xamarin.Auth;

namespace ContosoMoments
{
    public interface IPlatform
    {
        Task<string> GetDataFilesPath();

        Task<IMobileServiceFileDataSource> GetFileDataSource(MobileServiceFileMetadata metadata);

        Task DownloadFileAsync<T>(IMobileServiceSyncTable<T> table, MobileServiceFile file, string fullPath);

        string GetRootDataPath();

        Task<string> TakePhotoAsync(object context);

        Task<MobileServiceUser> LoginAsync(MobileServiceAuthenticationProvider provider);

        /// <summary>
        /// Login using Facebook client flow
        /// </summary>
        Task<MobileServiceUser> LoginFacebookAsync();

        AccountStore GetAccountStore();

        Task LogoutAsync();

        void LogEvent(string eventName);
    }
}
