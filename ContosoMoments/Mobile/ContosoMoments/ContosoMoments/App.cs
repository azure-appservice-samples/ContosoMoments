using ContosoMoments.Settings;
using ContosoMoments.Views;
using Microsoft.WindowsAzure.MobileServices;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ContosoMoments
{
    public class App : Application
    {
        public static MobileServiceClient MobileService;
        public static MobileServiceUser AuthenticatedUser;

        public static App Instance;
        //DEBUG
        //public ImageSource image = null;
        public Stream ImageStream = null;
        public event Action ShouldTakePicture = () => { };
        public event EventHandler ImageTaken;

        public App()
        {
            Instance = this;

            Label label = new Label() { Text = "Loading..." };
            label.TextColor = Color.White;
            Image img = new Image() {
                Source = Device.OnPlatform(
                    iOS: ImageSource.FromFile("Assets/logo.png"),
                    Android: ImageSource.FromFile("logo.png"),
                    WinPhone: ImageSource.FromFile("Assets/logo.png"))};
            StackLayout stack = new StackLayout();
            stack.VerticalOptions = LayoutOptions.Center;
            stack.HorizontalOptions = LayoutOptions.Center;
            stack.Orientation = StackOrientation.Vertical;
            stack.Children.Add(img);
            stack.Children.Add(label);
            ContentPage page = new ContentPage();
            page.BackgroundColor = Color.FromHex("#8C0A4B");
            page.Content = stack;
            MainPage = page;

        }

        protected override async void OnStart()
        {
            // Handle when your app starts
            if (AppSettings.Current.GetValueOrDefault<string>("MobileAppURL") == default(string))
            {
                //first run
                MainPage = new SettingView();
            }
            else
            {
                Constants.ApplicationURL = AppSettings.Current.GetValueOrDefault<string>("MobileAppURL");
                //Constants.GatewayURL = AppSettings.Current.GetValueOrDefault<string>("GatewayURL");
                bool isAuthRequred = await Utils.IsAuthRequired(Constants.ApplicationURL);

                ////Constants.ApplicationKey = AppSettings.Current.GetValueOrDefault<string>("ApplicationKey");
                //if (isAuthRequred)
                //    MobileService = new MobileServiceClient(Constants.ApplicationURL, Constants.GatewayURL, string.Empty);
                //else
                    MobileService = new MobileServiceClient(Constants.ApplicationURL);

                AuthenticatedUser = MobileService.CurrentUser;

#if !__WP__
                if (isAuthRequred && AuthenticatedUser == null)
                {
                    MainPage = new NavigationPage(new Login());
                }
                else
                {
                    // The root page of your application
                    MainPage = new NavigationPage(new ImagesList());
                }
#elif __WP__
                MainPage = new NavigationPage(new ImagesList());
#endif
            }
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }

        public async void ShowCapturedImage(string filepath)
        {

            FileStream fs = new FileStream(filepath, FileMode.Open);
            if (fs.CanRead)
            {
                byte[] buffer = new byte[fs.Length];
                await fs.ReadAsync(buffer, 0, (int)fs.Length);

                ImageStream = new MemoryStream(buffer);
            }

            //DEBUG
            //image = ImageSource.FromFile(filepath);

            if (null != ImageTaken)
                ImageTaken(this, new EventArgs());
        }

#if __WP__
        public async void ShowCapturedImage(Stream stream)
        {
            //DEBUG
            //image = ImageSource.FromStream(() => stream);

            byte[] bytes = new byte[(int)stream.Length];
            await stream.ReadAsync(bytes, 0, (int)stream.Length);

            ImageStream = new MemoryStream(bytes);

            if (null != ImageTaken)
                ImageTaken(this, new EventArgs());
        }
#endif

        public void TakePicture()
        {
            ShouldTakePicture();
        }
    }
}
