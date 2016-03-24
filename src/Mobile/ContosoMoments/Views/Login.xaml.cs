using ContosoMoments.Models;
using Microsoft.WindowsAzure.MobileServices;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ContosoMoments.Views
{
    public partial class Login : ContentPage
        {
        private TaskCompletionSource<Settings.AuthOption> tcs = new TaskCompletionSource<Settings.AuthOption>();

        public Login()
        {
            InitializeComponent();
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
            App.Instance.CurrentUserId = Settings.Current.DefaultUserId; // use default user ID
            await LoginComplete(Settings.AuthOption.GuestAccess);
        }

        private async Task LoginComplete(Settings.AuthOption option)
        {
            await Navigation.PopToRootAsync();

            Settings.Current.AuthenticationType = option;
            tcs.TrySetResult(option);
        }

        private async Task DoLoginAsync(MobileServiceAuthenticationProvider provider, Settings.AuthOption authOption)
        {
            MobileServiceUser user;

            try {
                user = await DependencyService.Get<IMobileClient>().LoginAsync(provider);
                App.Instance.AuthenticatedUser = user;
                System.Diagnostics.Debug.WriteLine("Authenticated with user: " + user.UserId);

                App.Instance.CurrentUserId =
                    await App.Instance.MobileService.InvokeApiAsync<string>(
                        "ManageUser",
                        System.Net.Http.HttpMethod.Get,
                        null);

                Debug.WriteLine($"Set current userID to: {App.Instance.CurrentUserId}");
                await LoginComplete(authOption);
            }
            catch (Exception) { // if user cancels, then set to Guest access
                Settings.Current.AuthenticationType = Settings.AuthOption.GuestAccess;
                tcs.TrySetResult(Settings.AuthOption.GuestAccess);
            }
        }
    }
}
