using System;
using System.Linq;
using ContosoMoments.Helpers;
using Microsoft.WindowsAzure.MobileServices;
using Xamarin.Forms;

namespace ContosoMoments.Views
{
    public partial class SettingsView : ContentPage
    {
        public bool UrlIsInvalid { get; set; }
        private App _app;

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
            mobileServiceUrl.Text = "https://donnamcontosomoments.azurewebsites.net/";

            if (Settings.MobileAppUrl != Settings.DefaultMobileAppUrl) {
                mobileServiceUrl.Text = Settings.MobileAppUrl;
            }

            if (UrlIsInvalid)
                DisplayAlert("Configuration Error", "Mobile Service URL seems to be not valid anymore. Please check the URL value and try again", "OK");


            base.OnAppearing();
        }

        public async void OnSave(object sender, EventArgs args)
        {
            Uri uri;
            string uriText = mobileServiceUrl.Text;

            if (!Uri.TryCreate(uriText, UriKind.Absolute, out uri)) {
                await DisplayAlert("Configuration Error", "Invalid URI entered", "OK");
                return;
            }

            if (Settings.MobileAppUrl != Settings.DefaultMobileAppUrl && Settings.MobileAppUrl != uriText) {
                // new URI was entered, and one had been set previously, so the app state should be reset
                Settings.MobileAppUrl = uriText;
                await _app.ResetAsync(uriText);
            }
            else {
                Settings.MobileAppUrl = uriText;
                await _app.InitMobileService(uriText);
                await Utils.PopulateDefaultsAsync();
            }
        }
    }
}
