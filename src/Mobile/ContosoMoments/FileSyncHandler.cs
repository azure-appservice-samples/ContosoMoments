using Microsoft.WindowsAzure.MobileServices.Files;
using Microsoft.WindowsAzure.MobileServices.Files.Metadata;
using Microsoft.WindowsAzure.MobileServices.Files.Sync;
using System.Diagnostics;
using System.Threading.Tasks;
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
                await FileHelper.DeleteLocalFileAsync(file, theApp.DataFilesPath);
            }
            else { // Create or update. We're aggressively downloading all files.
                Trace.WriteLine(string.Format("File - storeUri: {1}", file.Name, file.StoreUri));

                if (file.StoreUri.Contains("lg")) {
                    await this.theApp.DownloadFileAsync(file);                    
                }
            }
        }
    }
}
