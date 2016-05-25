using Microsoft.WindowsAzure.MobileServices;
using System;
using Xamarin.Forms;
using System.Threading.Tasks;

namespace ContosoMoments.Views
{
    public partial class SettingsView : ContentPage
    {
        public bool UrlIsInvalid { get; set; }
        private App _app;
        private TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();

        public SettingsView(App app)
        {
            InitializeComponent();
            _app = app;

            var tapSaveImage = new TapGestureRecognizer();
            tapSaveImage.Tapped += OnSave;
            imgSave.GestureRecognizers.Add(tapSaveImage);
        }

        protected override void OnAppearing()
        {
            if (Settings.Current.MobileAppUrl != Settings.DefaultMobileAppUrl) {
                mobileServiceUrl.Text = Settings.Current.MobileAppUrl;
                LogoutButton.IsEnabled = true;
            }

            if (UrlIsInvalid)
                DisplayAlert("Configuration Error", "Mobile Service URL seems to be not valid anymore. Please check the URL value and try again", "OK");


            base.OnAppearing();
        }

        public Task ShowDialog()
        {
            return tcs.Task;
        }

        public async void OnSave(object sender, EventArgs args)
        {
            if (Settings.Current.MobileAppUrl == mobileServiceUrl.Text &&
                Settings.Current.MobileAppUrl != Settings.DefaultMobileAppUrl) {
                // no changes, return
                await Navigation.PopModalAsync();
                tcs.TrySetResult(true);
                return;
            }

            string newUri;

            if (!GetHttpsUri(mobileServiceUrl.Text, out newUri)) {
                await DisplayAlert("Configuration Error", "Invalid URI entered", "OK");
                return;
            }

            if (Settings.Current.MobileAppUrl != Settings.DefaultMobileAppUrl) {
                // a URI had been set previously, so the app state should be reset
                Settings.Current.MobileAppUrl = newUri;
                await Navigation.PopModalAsync();
                tcs.TrySetResult(true);

                await _app.ResetAsync();
            }
            else {
                Settings.Current.MobileAppUrl = newUri;
            }
        }

        public async void OnLogout(object sender, EventArgs args)
        {
            await Navigation.PopModalAsync();
            await _app.LogoutAsync();
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
