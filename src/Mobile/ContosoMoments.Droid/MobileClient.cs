using Android.OS;
using Android.Webkit;
using ContosoMoments.Droid;
using ContosoMoments.Models;
using Microsoft.WindowsAzure.MobileServices;
using System.Threading.Tasks;
using Xamarin.Forms;

[assembly: Xamarin.Forms.Dependency(typeof(MobileClient))]
namespace ContosoMoments.Droid
{
    public class MobileClient : IMobileClient
    {
        public async Task<MobileServiceUser> LoginAsync(MobileServiceAuthenticationProvider provider)
        {
            return await App.MobileService.LoginAsync(Forms.Context, provider);
        }

        public async void Logout()
        {
            CookieManager.Instance.RemoveAllCookie();
            await App.MobileService.LogoutAsync();
        }

        public void ForceCloseApp()
        {
            Process.KillProcess(Process.MyPid());
        }
    }
}