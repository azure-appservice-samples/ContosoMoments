using ContosoMoments.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
#if __WP__
using Windows.Networking.Sockets;
#elif __DROID__
using Java.Net;
#elif __IOS__
using System.Net;
using System.Net.Sockets;
using System.Threading;
#endif
using Xamarin.Forms;

namespace ContosoMoments.Views
{
    public partial class SettingView : ContentPage
    {
        public SettingView()
        {
            InitializeComponent();

            var tapSaveImage = new TapGestureRecognizer();
            tapSaveImage.Tapped += OnSave;
            imgSave.GestureRecognizers.Add(tapSaveImage);
        }

        public async void OnSave(object sender, EventArgs args)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                mobileServiceUrl.Text = "http://contosomomentsmobileweb.azurewebsites.net/";
            }

            if (null != mobileServiceUrl.Text)
            {
                if (mobileServiceUrl.Text.Length != 0)
                {
                    bool ValidURL = false;

                    if (mobileServiceUrl.Text.Last() != '/')
                        mobileServiceUrl.Text += "/";

#if __WP__
                    ValidURL = await Utils.CheckServerAddressWP(mobileServiceUrl.Text);
#elif __IOS__
                ValidURL = await Utils.CheckServerAddressIOS(mobileServiceUrl.Text);
#elif __DROID__
                    ValidURL = await Utils.CheckServerAddressDroid(mobileServiceUrl.Text);
#endif

                    if (ValidURL)
                    {
                        AppSettings.Current.AddOrUpdateValue<string>("MobileAppURL", mobileServiceUrl.Text);
                        Constants.ApplicationURL = AppSettings.Current.GetValueOrDefault<string>("MobileAppURL");

                        string getawayURL = Constants.ApplicationURL + ".auth/login/facebook/callback";
                        getawayURL = getawayURL.Replace("http://", "https://");
                        AppSettings.Current.AddOrUpdateValue<string>("GatewayURL", getawayURL);
                        Constants.GatewayURL = AppSettings.Current.GetValueOrDefault<string>("GatewayURL");

                        bool isAuthRequred = await Utils.IsAuthRequired(Constants.ApplicationURL);

                        if (isAuthRequred)
                            App.MobileService = new Microsoft.WindowsAzure.MobileServices.MobileServiceClient(Constants.ApplicationURL, Constants.GatewayURL, string.Empty);
                        else
                            App.MobileService = new Microsoft.WindowsAzure.MobileServices.MobileServiceClient(Constants.ApplicationURL);

                        App.AuthenticatedUser = App.MobileService.CurrentUser;

#if !__WP__
                        if (isAuthRequred && App.AuthenticatedUser == null)
                        {
                            App.Current.MainPage = new NavigationPage(new Login());
                        }
                        else
                        {
                            // The root page of your application
                            App.Current.MainPage = new NavigationPage(new ImagesList());
                        }
#elif __WP__
                        App.Current.MainPage = new NavigationPage(new ImagesList());
#endif
                    }
                    else
                    {
                        DisplayAlert("Configuration Error", "Mobile Service URL is unreachable. Please check the URL value and try again", "OK");
                    }
                }
                else
                {
                    DisplayAlert("Configuration Error", "Mobile Service URL is empty. Please type in the value and try again", "OK");
                }
            }
            else
                DisplayAlert("Configuration Error", "Mobile Service URL is empty. Please type in the value and try again", "OK");
        }
    }
}
