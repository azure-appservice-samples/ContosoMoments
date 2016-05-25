using ContosoMoments.iOS;
using ContosoMoments.Models;
using Foundation;
using Microsoft.WindowsAzure.MobileServices;
using System.Threading.Tasks;
using UIKit;
using Facebook.LoginKit;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System;

[assembly: Xamarin.Forms.Dependency(typeof(MobileClient))]
namespace ContosoMoments.iOS
{
    public class MobileClient : IMobileClient
    {
        private TaskCompletionSource<MobileServiceUser> tcs;

        public Task<MobileServiceUser> LoginAsync(MobileServiceAuthenticationProvider provider)
        {
            var view = UIApplication.SharedApplication.KeyWindow.RootViewController;
            return App.Instance.MobileService.LoginAsync(view, provider);
        }

        public async Task Logout()
        {
            foreach (var cookie in NSHttpCookieStorage.SharedStorage.Cookies) {
                NSHttpCookieStorage.SharedStorage.DeleteCookie(cookie);
            }

            await App.Instance.MobileService.LogoutAsync();
        }

        public void ForceCloseApp()
        {
            System.Threading.Thread.CurrentThread.Abort();
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
