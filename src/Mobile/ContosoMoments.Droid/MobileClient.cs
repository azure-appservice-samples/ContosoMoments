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
        public Task<MobileServiceUser> LoginAsync(MobileServiceAuthenticationProvider provider)
        {
            return App.Instance.MobileService.LoginAsync(Forms.Context, provider);
        }

        public async Task Logout()
        {
            CookieManager.Instance.RemoveAllCookie();
            await App.Instance.MobileService.LogoutAsync();
        }

        public void ForceCloseApp()
        {
            Process.KillProcess(Process.MyPid());
        }

        public Task<MobileServiceUser> LoginFacebookAsync()
        {
            return LoginAsync(MobileServiceAuthenticationProvider.Facebook);
        }
    }
}