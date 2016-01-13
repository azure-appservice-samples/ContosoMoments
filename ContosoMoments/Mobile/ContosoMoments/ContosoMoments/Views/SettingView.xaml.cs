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
        public bool IsInURLTrouble { get; set; }

        public SettingView()
        {
            InitializeComponent();

            var tapSaveImage = new TapGestureRecognizer();
            tapSaveImage.Tapped += OnSave;
            imgSave.GestureRecognizers.Add(tapSaveImage);
        }

        protected override void OnAppearing()
        {
            if (null != AppSettings.Current.GetValueOrDefault<string>("MobileAppURL"))
                mobileServiceUrl.Text = AppSettings.Current.GetValueOrDefault<string>("MobileAppURL");

            if (IsInURLTrouble)
                DisplayAlert("Configuration Error", "Mobile Service URL seems to be not valid anymore. Please check the URL value and try again", "OK");

            base.OnAppearing();
        }

        public async void OnSave(object sender, EventArgs args)
        {
            //if (System.Diagnostics.Debugger.IsAttached && null == mobileServiceUrl.Text)
            //{
            //    mobileServiceUrl.Text = "http://contosomomentsqa.azurewebsites.net/";
            //}

            if (null != mobileServiceUrl.Text)
            {
                if (mobileServiceUrl.Text.Length != 0)
                {
                    bool ValidURL = false;

                    if (mobileServiceUrl.Text.Last() != '/')
                        mobileServiceUrl.Text += "/";

                    if (mobileServiceUrl.Text.IndexOf("http://") == -1 && mobileServiceUrl.Text.IndexOf("https://") == -1)
                    {
                        DisplayAlert("Configuration Error", "Mobile Service URL does not contain http or https scheme. Please check the URL value and try again", "OK");
                        return;
                    }

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

                        bool isAuthRequred = await Utils.IsAuthRequired(Constants.ApplicationURL);

                        App.MobileService = new Microsoft.WindowsAzure.MobileServices.MobileServiceClient((!isAuthRequred ? Constants.ApplicationURL : Constants.ApplicationURL.Replace("http://", "https://")));
                        App.AuthenticatedUser = App.MobileService.CurrentUser;

                        await (App.Current as App).InitLocalStoreAsync(App.DB_LOCAL_FILENAME);
                        (App.Current as App).InitLocalTables();

                        if (isAuthRequred && App.AuthenticatedUser == null)
                        {
                            App.Current.MainPage = new NavigationPage(new Login());
                        }
                        else
                        {
#if __DROID__
                            Droid.GcmService.RegisterWithMobilePushNotifications();
#elif __IOS__
                            iOS.AppDelegate.IsAfterLogin = true;
                            await iOS.AppDelegate.RegisterWithMobilePushNotifications();
#elif __WP__
                            ContosoMoments.WinPhone.App.AcquirePushChannel(App.MobileService);
#endif
                            // The root page of your application
                            //App.Current.MainPage = new NavigationPage(new ImagesList());
                            App.Current.MainPage = new NavigationPage(new AlbumsListView());
                        }
                    }
                    else
                    {
                        DisplayAlert("Configuration Error", "Mobile Service URL malformed, unreachable or does not expose ContosoMoments Web APIs. Please check the URL value and try again", "OK");
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
