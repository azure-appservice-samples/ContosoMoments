using Microsoft.WindowsAzure.MobileServices;
using System;
using Xamarin.Forms;
using System.Threading.Tasks;
using System.Diagnostics;

namespace ContosoMoments.Views
{
    public partial class SettingsView : ContentPage
    {
        private App app;
        private TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
        
        private const string PrivacyUri = "https://go.microsoft.com/fwlink/?LinkId=521839";
        private const string AboutUri = "https://contosomoments.azurewebsites.net/about";

        public SettingsView(App app)
        {
            InitializeComponent();
            this.app = app;
        }

        protected override void OnAppearing()
        {
            mobileServiceUrl.Text = Settings.Current.MobileAppUrl;

            if (!Settings.IsFirstStart()) {
                LogoutButton.IsEnabled = true;
            }

            base.OnAppearing();
        }

        /// Returns true if the App URL was changed
        public Task<bool> ShowDialog()
        {
            return tcs.Task;
        }

        public async void OnSave(object sender, EventArgs args)
        {
            string newUri;

            // convert to HTTPS
            if (!GetHttpsUri(mobileServiceUrl.Text, out newUri)) {
                await DisplayAlert("Configuration Error", "Invalid URI entered", "OK");
                return;
            }

            if (Settings.Current.MobileAppUrl == mobileServiceUrl.Text || Settings.Current.MobileAppUrl == newUri) {
                Settings.Current.MobileAppUrl = newUri; // save the URL, in case the scheme needed to be changed

                // no changes, return
                await Navigation.PopModalAsync();
                tcs.TrySetResult(false);
            }
            else {
                DependencyService.Get<IPlatform>().LogEvent("MobileAppUrlChanged");

                Settings.Current.MobileAppUrl = newUri;

                await Navigation.PopModalAsync();
                tcs.TrySetResult(true);
            }
        }

        public void OnPrivacyButtonClicked(object sender, EventArgs args)
        {
            // open link to privacy policy
            Device.OpenUri(new System.Uri(PrivacyUri));
        }

        public async void OnLogout(object sender, EventArgs args)
        {
            await Navigation.PopModalAsync();
            await app.LogoutAsync();
        }

        public void OnLearnMoreButtonClicked(object sender, EventArgs args)
        {
            Device.OpenUri(new System.Uri(AboutUri));
        }

        private bool GetHttpsUri(string inputString, out string httpsUri)
        {
            if (!Uri.IsWellFormedUriString(inputString, UriKind.Absolute)) {
                httpsUri = "";
                return false;
            }

            var uriBuilder = new UriBuilder(inputString) {
                Scheme = Uri.UriSchemeHttps,
                Port = -1
            }; // set as https always

            httpsUri = uriBuilder.ToString();
            return true;
        }
    }
}
