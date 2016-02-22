using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices.Files;
using Microsoft.WindowsAzure.MobileServices.Files.Metadata;
using Microsoft.WindowsAzure.MobileServices.Files.Sync;
using Microsoft.WindowsAzure.MobileServices.Sync;
using Xamarin.Media;

[assembly: Xamarin.Forms.Dependency(typeof(ContosoMoments.iOS.TouchPlatform))]
namespace ContosoMoments.iOS
{
    class TouchPlatform : IPlatform
    {
        public async Task DownloadFileAsync<T>(IMobileServiceSyncTable<T> table, MobileServiceFile file, string fullPath)
        {
            await table.DownloadFileAsync(file, fullPath);
        }

        public async Task<IMobileServiceFileDataSource> GetFileDataSource(MobileServiceFileMetadata metadata)
        {
            var filePath = await FileHelper.GetLocalFilePathAsync(metadata.ParentDataItemId, metadata.FileName, await GetDataFilesPath());
            return new PathMobileServiceFileDataSource(filePath);
        }

        public Task<string> GetDataFilesPath()
        {
            string filesPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ContosoImages");

            if (!Directory.Exists(filesPath)) {
                Directory.CreateDirectory(filesPath);
            }

            return Task.FromResult(filesPath);
        }

        public string GetDataPathAsync()
        {
            return Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
        }

        public async Task<string> TakePhotoAsync(object context)
        {
            try {
                var mediaPicker = new MediaPicker();
                var mediaFile = await mediaPicker.PickPhotoAsync();
                return mediaFile.Path;
            }
            catch (TaskCanceledException) {
                return null;
            }
        }
    }
}