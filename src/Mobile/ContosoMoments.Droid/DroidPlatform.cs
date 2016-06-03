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
using Xamarin.Auth;
using ContosoMoments.Helpers;
using Xamarin.Facebook.AppEvents;
using Xamarin.Facebook;
using Xamarin.Facebook.Login;
using Android.App;
using Newtonsoft.Json.Linq;
using System.Diagnostics;

[assembly: Xamarin.Forms.Dependency(typeof(ContosoMoments.Droid.DroidPlatform))]
namespace ContosoMoments.Droid
{
    public class DroidPlatform : IPlatform
    {
        private TaskCompletionSource<MobileServiceUser> tcs; // used in LoginFacebookAsync
        private AppEventsLogger logger = AppEventsLogger.NewLogger(MainActivity.DefaultService.Application);

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

        public async Task<MobileServiceUser> LoginAsync(MobileServiceAuthenticationProvider provider)
        {
            // login doesn't need to cache the user, since it will be cached by the caller

            var user = GetCachedUser();

            if (user == null) {
                user = await App.Instance.MobileService.LoginAsync(Forms.Context, provider);
            }

            return user;
        }

        public async Task LogoutAsync()
        {
            LoginManager.Instance.LogOut();

            CookieManager.Instance.RemoveAllCookie();
            AuthStore.DeleteTokenCache();
            await App.Instance.MobileService.LogoutAsync();
        }

        public Task<MobileServiceUser> LoginFacebookAsync()
        {
            tcs = new TaskCompletionSource<MobileServiceUser>();
            MainActivity.DefaultService.SetPlatformCallback(this); // set context for facebook callbacks
            LoginManager.Instance.LogInWithReadPermissions(App.UIContext as Activity, new[] { "public_profile" } );

            return tcs.Task;
        }

        internal async Task OnFacebookLoginSuccess(string tokenString)
        {
            Debug.WriteLine($"Logged into Facebook, access_token: {tokenString}");

            var token = new JObject(); 
            token["access_token"] = tokenString;

            var user = await App.Instance.MobileService.LoginAsync(MobileServiceAuthenticationProvider.Facebook, token);
            Debug.WriteLine($"Logged into MobileService, user: {user.UserId}");

            tcs.TrySetResult(user);
        }

        internal void OnFacebookLoginError(FacebookException e)
        {
            tcs.TrySetException(e);
        }

        internal void OnFacebookLoginCancel()
        {
            tcs.TrySetException(new Exception("Facebook login cancelled"));
        }

        public AccountStore GetAccountStore()
        {
            return AccountStore.Create(Forms.Context);
        }

        private MobileServiceUser GetCachedUser()
        {
            var user = AuthStore.GetUserFromCache();
            if (user != null) {
                App.Instance.MobileService.CurrentUser = user;
            }

            return user;
        }

        public void LogEvent(string eventName)
        {
            logger.LogEvent(eventName);
        }
    }
}