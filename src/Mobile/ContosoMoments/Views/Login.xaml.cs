using ContosoMoments.Models;
using Microsoft.WindowsAzure.MobileServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ContosoMoments.Views
{
	public partial class Login : ContentPage
	{
		public Login ()
		{
			InitializeComponent ();
		}

        async void OnFBLoginClicked(object sender, EventArgs e)
        {
            await DoLoginAsync(MobileServiceAuthenticationProvider.Facebook);
        }

        async void OnAADLoginClicked(object sender, EventArgs e)
        {
            await DoLoginAsync(MobileServiceAuthenticationProvider.WindowsAzureActiveDirectory);
        }

        private async Task DoLoginAsync(MobileServiceAuthenticationProvider provider)
        {
            MobileServiceUser user;

            try
            {
                user = await DependencyService.Get<IMobileClient>().LoginAsync(provider);
                App.AuthenticatedUser = user;
                System.Diagnostics.Debug.WriteLine("Authenticated with user: " + user.UserId);

#if __DROID__
                Droid.GcmService.RegisterWithMobilePushNotifications();
#elif __IOS__
                iOS.AppDelegate.IsAfterLogin = true;
                await iOS.AppDelegate.RegisterWithMobilePushNotifications();
#elif __WP__
                ContosoMoments.WinPhone.App.AcquirePushChannel(App.MobileService);
#endif

                Navigation.InsertPageBefore(new AlbumsListView(App.Current as App), this);
                await Navigation.PopAsync();
            }
            catch (InvalidOperationException ex)
            {
                if (ex.Message.Contains("Authentication was cancelled"))
                {
                    messageLabel.Text = "Authentication cancelled by the user";
                }
            }
            catch (Exception)
            {
                messageLabel.Text = "Authentication failed";
            }
        }
    }
}
