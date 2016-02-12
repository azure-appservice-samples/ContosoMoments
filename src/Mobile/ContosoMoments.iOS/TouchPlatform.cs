using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices.Files;
using Microsoft.WindowsAzure.MobileServices.Files.Metadata;
using Microsoft.WindowsAzure.MobileServices.Files.Sync;
using Microsoft.WindowsAzure.MobileServices.Sync;

[assembly: Xamarin.Forms.Dependency(typeof(ContosoMoments.iOS.TouchPlatform))]
namespace ContosoMoments.iOS
{
    class TouchPlatform : IPlatform
    {
        public async Task DownloadFileAsync<T>(IMobileServiceSyncTable<T> table, MobileServiceFile file, string filename)
        {
            var path = await FileHelper.GetLocalFilePathAsync(file.ParentId, file.Name);
            await table.DownloadFileAsync(file, path);
        }

        public async Task<IMobileServiceFileDataSource> GetFileDataSource(MobileServiceFileMetadata metadata)
        {
            var filePath = await FileHelper.GetLocalFilePathAsync(metadata.ParentDataItemId, metadata.FileName);
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
    }
}