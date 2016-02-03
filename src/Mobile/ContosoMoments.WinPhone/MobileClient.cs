using ContosoMoments.Models;
using ContosoMoments.WinPhone;
using Microsoft.WindowsAzure.MobileServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;

[assembly: Xamarin.Forms.Dependency(typeof(MobileClient))]
namespace ContosoMoments.WinPhone
{
    public class MobileClient : IMobileClient
    {
        public async Task<MobileServiceUser> LoginAsync(MobileServiceAuthenticationProvider provider)
        {
            IDictionary<string, string> p = new Dictionary<string, string>();

            return await ContosoMoments.App.MobileService.LoginAsync(provider.ToString(), p);
        }

        public async void Logout()
        {
            await ContosoMoments.App.MobileService.LogoutAsync();
        }

        public static void LoginComplete(WebAuthenticationBrokerContinuationEventArgs args)
        {
            //ContosoMoments.App.MobileService.LoginComplete(args);
        }

        public void ForceCloseApp()
        {
            App.Current.Terminate();
        }
    }
}
