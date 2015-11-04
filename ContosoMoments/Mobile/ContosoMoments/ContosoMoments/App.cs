using ContosoMoments.Views;
using Microsoft.WindowsAzure.MobileServices;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Xamarin.Forms;

namespace ContosoMoments
{
    public class App : Application
    {
        public static MobileServiceClient MobileService = new MobileServiceClient(Constants.ApplicationURL/*, Constants.ApplicationKey*/);

        public static App Instance;
        //DEBUG
        //public ImageSource image = null;
        public Stream ImageStream = null;
        public event Action ShouldTakePicture = () => { };
        public event EventHandler ImageTaken;

        public App()
        {
            Instance = this;

            // The root page of your application
            //MainPage = new ContentPage {
            //	Content = new StackLayout {
            //		VerticalOptions = LayoutOptions.Center,
            //		Children = {
            //			new Label {
            //				XAlign = TextAlignment.Center,
            //				Text = "Welcome to Xamarin Forms!"
            //			}
            //		}
            //	}
            //};
            MainPage = new NavigationPage(new ImagesList());
        }

        protected override void OnStart()
        {
            // Handle when your app starts
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

            ImageStream = new MemoryStream((int)stream.Length);
            await stream.CopyToAsync(ImageStream);

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
