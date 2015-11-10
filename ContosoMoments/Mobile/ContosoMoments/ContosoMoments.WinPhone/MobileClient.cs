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
            return await ContosoMoments.App.MobileService.LoginAsync(provider);
        }

        public void Logout()
        {
            ContosoMoments.App.MobileService.Logout();
        }

        public static void LoginComplete(WebAuthenticationBrokerContinuationEventArgs args)
        {
            //ContosoMoments.App.MobileService.LoginComplete(args);
        }
    }
}
