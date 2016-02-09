using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices.Files.Sync;
using Microsoft.WindowsAzure.MobileServices.Files;
using Microsoft.WindowsAzure.MobileServices.Files.Metadata;
using Xamarin.Forms;

namespace ContosoMoments
{
    public class FileSyncHandler : IFileSyncHandler
    {
        private readonly App theApp;

        public FileSyncHandler(App app)
        {
            this.theApp = app;
        }

        public Task<IMobileServiceFileDataSource> GetDataSource(MobileServiceFileMetadata metadata)
        {
            IPlatform platform = DependencyService.Get<IPlatform>();
            return platform.GetFileDataSource(metadata);
        }

        public async Task ProcessFileSynchronizationAction(MobileServiceFile file, FileSynchronizationAction action)
        {
            if (action == FileSynchronizationAction.Delete) {
                await FileHelper.DeleteLocalFileAsync(file);
            }
            else { // Create or update. We're aggressively downloading all files.
                await this.theApp.DownloadFileAsync(file);
            }
        }
    }
}
