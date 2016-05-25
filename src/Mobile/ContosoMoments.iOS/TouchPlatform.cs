using Microsoft.WindowsAzure.MobileServices.Files;
using Microsoft.WindowsAzure.MobileServices.Files.Metadata;
using Microsoft.WindowsAzure.MobileServices.Files.Sync;
using Microsoft.WindowsAzure.MobileServices.Sync;
using System;
using System.IO;
using System.Threading.Tasks;
using Xamarin.Media;
using Microsoft.WindowsAzure.MobileServices;
using Foundation;
using Facebook.LoginKit;
using UIKit;
using System.Diagnostics;
using Newtonsoft.Json.Linq;

[assembly: Xamarin.Forms.Dependency(typeof(ContosoMoments.iOS.TouchPlatform))]
namespace ContosoMoments.iOS
{
    class TouchPlatform : IPlatform
    {
        private TaskCompletionSource<MobileServiceUser> tcs; // used in LoginFacebookAsync

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

        public string GetRootDataPath()
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

        public Task<MobileServiceUser> LoginAsync(MobileServiceAuthenticationProvider provider)
        {
            var view = UIApplication.SharedApplication.KeyWindow.RootViewController;
            return App.Instance.MobileService.LoginAsync(view, provider);
        }

        public async Task LogoutAsync()
        {
            foreach (var cookie in NSHttpCookieStorage.SharedStorage.Cookies) {
                NSHttpCookieStorage.SharedStorage.DeleteCookie(cookie);
            }

            await App.Instance.MobileService.LogoutAsync();
        }

        public Task<MobileServiceUser> LoginFacebookAsync()
        {
            tcs = new TaskCompletionSource<MobileServiceUser>();
            var loginManager = new LoginManager();
            var view = UIApplication.SharedApplication.KeyWindow.RootViewController;

            Debug.WriteLine("Starting Facebook client flow");
            loginManager.LogInWithReadPermissions(new[] { "public_profile" }, view, LoginTokenHandler);

            return tcs.Task;
        }

        private async void LoginTokenHandler(LoginManagerLoginResult loginResult, NSError error)
        {
            if (loginResult.Token != null) {
                Debug.WriteLine($"Logged into Facebook, access_token: {loginResult.Token.TokenString}");

                var token = new JObject();
                token["access_token"] = loginResult.Token.TokenString;

                var user = await App.Instance.MobileService.LoginAsync(MobileServiceAuthenticationProvider.Facebook, token);
                Debug.WriteLine($"Logged into MobileService, user: {user.UserId}");

                tcs.TrySetResult(user);
            }
            else {
                tcs.TrySetException(new Exception("Facebook login failed"));
            }
        }

    }
}