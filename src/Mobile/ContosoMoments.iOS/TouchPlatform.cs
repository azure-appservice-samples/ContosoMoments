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
using Xamarin.Auth;
using System.Linq;
using ContosoMoments.Helpers;

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
            string filesPath = Path.Combine(GetRootDataPath(), "ContosoImages");

            if (!Directory.Exists(filesPath)) {
                Directory.CreateDirectory(filesPath);
            }

            return Task.FromResult(filesPath);
        }

        public string GetRootDataPath()
        {
            // return a reference to <Application_Home>/Library/Caches, so that the images are not marked for iCloud backup
            var documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            return Path.Combine(documents, "..", "Library", "Caches"); 
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

        public async Task<MobileServiceUser> LoginAsync(MobileServiceAuthenticationProvider provider)
        {
            // login doesn't need to cache the user, since it will be cached by the caller

            var user = GetCachedUser();

            if (user == null) {
                var view = UIApplication.SharedApplication.KeyWindow.RootViewController;
                user = await App.Instance.MobileService.LoginAsync(view, provider);
            }

            return user;
        }

        public async Task<MobileServiceUser> LoginFacebookAsync()
        {
            // login doesn't need to cache the user, since it will be cached by the caller

            tcs = new TaskCompletionSource<MobileServiceUser>();
            var loginManager = new LoginManager();
            var view = UIApplication.SharedApplication.KeyWindow.RootViewController;

            var user = GetCachedUser();

            if (user != null) {
                tcs.TrySetResult(user);
            }
            else { 
                Debug.WriteLine("Starting Facebook client flow");
                loginManager.LogInWithReadPermissions(new[] { "public_profile" }, view, LoginTokenHandler);
            }

            return await tcs.Task;
        }

        private MobileServiceUser GetCachedUser()
        {
            var user = AuthStore.GetUserFromCache();
            if (user != null) {
                App.Instance.MobileService.CurrentUser = user;
            }

            return user;
        }

        public async Task LogoutAsync()
        {
            foreach (var cookie in NSHttpCookieStorage.SharedStorage.Cookies) {
                NSHttpCookieStorage.SharedStorage.DeleteCookie(cookie);
            }

            AuthStore.DeleteTokenCache();
            await App.Instance.MobileService.LogoutAsync();
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

        public AccountStore GetAccountStore()
        {
            return AccountStore.Create();
        }

        public void LogEvent(string eventName)
        {
            Facebook.CoreKit.AppEvents.LogEvent(eventName);
        }
    }
}