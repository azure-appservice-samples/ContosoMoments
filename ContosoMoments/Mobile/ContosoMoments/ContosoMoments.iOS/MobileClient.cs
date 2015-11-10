using ContosoMoments.iOS;
using ContosoMoments.Models;
using Foundation;
using Microsoft.WindowsAzure.MobileServices;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UIKit;

[assembly: Xamarin.Forms.Dependency(typeof(MobileClient))]
namespace ContosoMoments.iOS
{
    public class MobileClient : IMobileClient
    {
        public async Task<MobileServiceUser> LoginAsync(MobileServiceAuthenticationProvider provider)
        {
            var view = UIApplication.SharedApplication.KeyWindow.RootViewController;
            return await App.MobileService.LoginAsync(view, provider);
        }

        public void Logout()
        {
            foreach (var cookie in NSHttpCookieStorage.SharedStorage.Cookies)
            {
                NSHttpCookieStorage.SharedStorage.DeleteCookie(cookie);
            }

            App.MobileService.Logout();
        }
    }
}
