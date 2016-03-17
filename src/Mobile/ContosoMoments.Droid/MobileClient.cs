using Android.OS;
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
            return await App.Instance.MobileService.LoginAsync(Forms.Context, provider);
        }

        public async void Logout()
        {
            CookieManager.Instance.RemoveAllCookie();
            await App.Instance.MobileService.LogoutAsync();
        }

        public void ForceCloseApp()
        {
            Process.KillProcess(Process.MyPid());
        }
    }
}