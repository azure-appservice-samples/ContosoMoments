using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Java.IO;
using System;
using Xamarin.Facebook;
using Xamarin.Facebook.AppEvents;
using Xamarin.Facebook.Login;
using Xamarin.Forms;
using Android.Content;

namespace ContosoMoments.Droid
{
    [Activity (Label = "Contoso Moments", Icon = "@drawable/icon", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsApplicationActivity
    {
        public static MainActivity instance;
        private ICallbackManager callbackManager;
        private FacebookCallback facebookCallback = new FacebookCallback();

        protected override void OnCreate (Bundle bundle)
        {
            base.OnCreate (bundle);

            FacebookSdk.SdkInitialize(ApplicationContext);
            AppEventsLogger.ActivateApp(Application);

            callbackManager = CallbackManagerFactory.Create();
            LoginManager.Instance.RegisterCallback(callbackManager, facebookCallback);

            this.Window.AddFlags(WindowManagerFlags.Fullscreen);

            global::Xamarin.Forms.Forms.Init(this, bundle);

            Microsoft.WindowsAzure.MobileServices.CurrentPlatform.Init();

            App.UIContext = this;
            LoadApplication(new ContosoMoments.App());

            instance = this;

#if PUSH // need to use a Google image on an Android emulator
            try {
                // Check to ensure everything's setup right
                GcmClient.CheckDevice(this);
                GcmClient.CheckManifest(this);

                // Register for push notifications
                System.Diagnostics.Debug.WriteLine("Registering...");
                GcmClient.Register(this, PushHandlerBroadcastReceiver.SENDER_IDS);
            }
            catch (Java.Net.MalformedURLException) {

                CreateAndShowDialog(new Exception("There was an error creating the Mobile Service. Verify the URL"), "Error");
            }
            catch (Exception e) {
                CreateAndShowDialog(e, "Error");
            }
#endif 
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            callbackManager.OnActivityResult(requestCode, (int)resultCode, data);
        }

        public static MainActivity DefaultService
        {
            get { return instance; }
        }

        private void CreateAndShowDialog(Exception e, string title)
        {
            //set alert for executing the task
            AlertDialog.Builder alert = new AlertDialog.Builder(this);

            alert.SetTitle(title);
            alert.SetMessage(e.Message);

            alert.SetPositiveButton("OK", (senderAlert, args) => { });

            //run the alert in UI thread to display in the screen
            RunOnUiThread(() => {
                alert.Show();
            });
        }

        internal void SetPlatformCallback(DroidPlatform platform)
        {
            facebookCallback.platform = platform;
        }
    }

    class FacebookCallback : Java.Lang.Object, IFacebookCallback
    {
        internal DroidPlatform platform;       

        public void OnCancel()
        {
            platform.OnFacebookLoginCancel();
        }

        public void OnError(FacebookException e)
        {
            platform.OnFacebookLoginError(e);
        }

        public async void OnSuccess(Java.Lang.Object obj)
        {
            LoginResult loginResult = (LoginResult) obj;
            await platform.OnFacebookLoginSuccess(loginResult.AccessToken.Token);
        }
    }
}

