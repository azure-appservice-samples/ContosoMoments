using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using ContosoMoments.Droid;
using ContosoMoments.Models;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices;
using Xamarin.Forms;
using Android.Webkit;

[assembly: Xamarin.Forms.Dependency(typeof(MobileClient))]
namespace ContosoMoments.Droid
{
    public class MobileClient : IMobileClient
    {
        public async Task<MobileServiceUser> LoginAsync(MobileServiceAuthenticationProvider provider)
        {
            //MobileServiceUser user = null;

            //try
            //{
            //    user = await App.MobileService.LoginAsync(Forms.Context, provider);
            //}
            //catch (Exception ex)
            //{
            //}

            //return user;
            return await App.MobileService.LoginAsync(Forms.Context, provider);
        }

        public async void Logout()
        {
            CookieManager.Instance.RemoveAllCookie();
            await App.MobileService.LogoutAsync();
        }
    }
}