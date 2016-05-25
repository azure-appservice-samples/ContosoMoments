using Android.Content;
using Microsoft.WindowsAzure.MobileServices.Files;
using Microsoft.WindowsAzure.MobileServices.Files.Metadata;
using Microsoft.WindowsAzure.MobileServices.Files.Sync;
using Microsoft.WindowsAzure.MobileServices.Sync;
using System;
using System.IO;
using System.Threading.Tasks;
using Xamarin.Media;
using Microsoft.WindowsAzure.MobileServices;
using Android.Webkit;
using Xamarin.Forms;

[assembly: Xamarin.Forms.Dependency(typeof(ContosoMoments.Droid.DroidPlatform))]
namespace ContosoMoments.Droid
{
    public class DroidPlatform : IPlatform
    {
        public async Task DownloadFileAsync<T>(IMobileServiceSyncTable<T> table, MobileServiceFile file, string fullPath)
        {
            await table.DownloadFileAsync(file, fullPath);
        }

        public async Task<IMobileServiceFileDataSource> GetFileDataSource(MobileServiceFileMetadata metadata)
        {
            var filePath = 
                await FileHelper.GetLocalFilePathAsync(
                    metadata.ParentDataItemId, metadata.FileName, dataFilesPath: await GetDataFilesPath());
            return new PathMobileServiceFileDataSource(filePath);
        }

        public string GetRootDataPath()
        {
            // TODO: Windows needs instead Windows.Storage.ApplicationData.Current.LocalFolder.Path
            return Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
        }

        public Task<string> GetDataFilesPath()
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string filesPath = Path.Combine(appData, "ContosoImages");

            if (!Directory.Exists(filesPath)) {
                Directory.CreateDirectory(filesPath);
            }

            return Task.FromResult(filesPath);
        }

        public async Task<string> TakePhotoAsync(object context)
        {
            try {
                var uiContext = context as Context;
                if (uiContext != null) {
                    var mediaPicker = new MediaPicker(uiContext);
                    var photo = await mediaPicker.TakePhotoAsync(new StoreCameraMediaOptions());

                    return photo.Path;
                }
            }
            catch (TaskCanceledException) {
            }

            return null;
        }

        public Task<MobileServiceUser> LoginAsync(MobileServiceAuthenticationProvider provider)
        {
            return App.Instance.MobileService.LoginAsync(Forms.Context, provider);
        }

        public async Task LogoutAsync()
        {
            CookieManager.Instance.RemoveAllCookie();
            await App.Instance.MobileService.LogoutAsync();
        }

        public Task<MobileServiceUser> LoginFacebookAsync()
        {
            return LoginAsync(MobileServiceAuthenticationProvider.Facebook);
        }
    }
}