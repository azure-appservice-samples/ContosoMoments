using ContosoMoments.Helpers;
using ContosoMoments.Models;
using Microsoft.WindowsAzure.MobileServices;
using System;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ContosoMoments.Views
{
    public partial class Login : ContentPage
	{
        private TaskCompletionSource<Settings.AuthOption> tcs = new TaskCompletionSource<Settings.AuthOption>();
        private Settings.AuthOption authOption;

		public Login ()
		{
			InitializeComponent ();
		}

        public Task<Settings.AuthOption> GetResultAsync()
        {
            return tcs.Task;
        }

        private async void OnFBLoginClicked(object sender, EventArgs e)
        {
            await DoLoginAsync(
                MobileServiceAuthenticationProvider.Facebook, 
                Settings.AuthOption.Facebook);
        }

        private async void OnAADLoginClicked(object sender, EventArgs e)
        {
            await DoLoginAsync(
                MobileServiceAuthenticationProvider.WindowsAzureActiveDirectory, 
                Settings.AuthOption.ActiveDirectory);
        }

        private async void OnGuestClicked(object sender, EventArgs e)
        {
            await LoginComplete(Settings.AuthOption.GuestAccess);
        }

        private async Task LoginComplete(Settings.AuthOption option)
        {
            await Navigation.PopToRootAsync();
            tcs.SetResult(option);
        }

        private async Task DoLoginAsync(MobileServiceAuthenticationProvider provider, Settings.AuthOption authOption)
        {
            MobileServiceUser user;

            try
            {
                user = await DependencyService.Get<IMobileClient>().LoginAsync(provider);
                App.Instance.AuthenticatedUser = user;
                System.Diagnostics.Debug.WriteLine("Authenticated with user: " + user.UserId);

                await App.Instance.MobileService.InvokeApiAsync<string>("ManageUser", System.Net.Http.HttpMethod.Get, null);

                await LoginComplete(authOption);
            }
            catch (InvalidOperationException ex)
            {
                if (ex.Message.Contains("Authentication was cancelled"))
                {
                    messageLabel.Text = "Authentication cancelled by the user";
                }

                tcs.SetException(ex);
            }
            catch (Exception e)
            {
                messageLabel.Text = "Authentication failed";
                tcs.SetException(e);
            }
        }
    }
}
