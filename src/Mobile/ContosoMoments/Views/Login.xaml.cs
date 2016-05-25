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
        private TaskCompletionSource<Settings.AuthOption> tcs;

        public Login()
        {
            InitializeComponent();
        }

        public Task<Settings.AuthOption> GetResultAsync()
        {
            tcs = new TaskCompletionSource<Settings.AuthOption>();
            return tcs.Task;
        }

        private async void OnFBLoginClicked(object sender, EventArgs e)
        {
            await DoLoginAsync(Settings.AuthOption.Facebook);
        }

        private async void OnAADLoginClicked(object sender, EventArgs e)
        {
            await DoLoginAsync(Settings.AuthOption.ActiveDirectory);
        }

        private async void OnGuestClicked(object sender, EventArgs e)
        {
            App.Instance.CurrentUserId = Settings.Current.DefaultUserId; // use default user ID
            await LoginComplete(Settings.AuthOption.GuestAccess);
        }

        private async Task LoginComplete(Settings.AuthOption option)
        {
            await Navigation.PopToRootAsync();

            tcs.TrySetResult(option);
        }

        private async Task DoLoginAsync(Settings.AuthOption authOption)
        {
            try
            {
                await AuthHandler.DoLoginAsync(authOption);
                await LoginComplete(authOption);
            }
            catch (Exception)
            { 
                // if user cancels, then set to Guest access
                await LoginComplete(Settings.AuthOption.GuestAccess);
            }
        }
    }
}
