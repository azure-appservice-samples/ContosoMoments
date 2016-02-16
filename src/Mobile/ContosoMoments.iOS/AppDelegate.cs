using System;
using System.Collections.Generic;
using System.Linq;

using Foundation;
using UIKit;
using System.IO;
using Microsoft.WindowsAzure.MobileServices;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Xamarin.Media;

namespace ContosoMoments.iOS
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the 
    // User Interface of the application, as well as listening (and optionally responding) to 
    // application events from iOS.
    [Register("AppDelegate")]
    public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
    {
        public static NSData DeviceToken { get; private set; }
        public static bool IsAfterLogin = false;

        //
        // This method is invoked when the application has loaded and is ready to run. In this 
        // method you should instantiate the window, load the UI into it and then make the window
        // visible.
        //
        // You have 17 seconds to return from this method, or iOS will terminate your application.
        //
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            app.StatusBarHidden = true;

            global::Xamarin.Forms.Forms.Init();

            Microsoft.WindowsAzure.MobileServices.CurrentPlatform.Init();
            SQLitePCL.CurrentPlatform.Init();

            if (UIDevice.CurrentDevice.CheckSystemVersion(8, 0))
            {
                var settings = UIUserNotificationSettings.GetSettingsForTypes(UIUserNotificationType.Sound |
                                                                              UIUserNotificationType.Alert |
                                                                              UIUserNotificationType.Badge, null);

                UIApplication.SharedApplication.RegisterUserNotificationSettings(settings);
                UIApplication.SharedApplication.RegisterForRemoteNotifications();

            }
            else
            {
                UIApplication.SharedApplication.RegisterForRemoteNotificationTypes(UIRemoteNotificationType.Badge |
                                                                                   UIRemoteNotificationType.Sound |
                                                                                   UIRemoteNotificationType.Alert);
            }
                
            var formsApp = new ContosoMoments.App();
            ChoosePhotoAsync(Xamarin.Forms.Application.Current as App);

            LoadApplication(formsApp);

            // comment when running on emulator
            // TakePhotoAsync(Xamarin.Forms.Application.Current as App);
        
            return base.FinishedLaunching(app, options);
        }

        private void TakePhotoAsync(UIApplication app, App formsApp)
        {
            var imagePicker = new UIImagePickerController { SourceType = UIImagePickerControllerSourceType.Camera };
            formsApp.ShouldTakePicture += () =>
                app.KeyWindow.RootViewController.PresentViewController(imagePicker, true, null);

            imagePicker.FinishedPickingMedia += (sender, e) =>
            {
                var filepath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "tmp.png");
                var image = (UIImage)e.Info.ObjectForKey(new NSString("UIImagePickerControllerOriginalImage"));
                InvokeOnMainThread(() =>
                {
                    image.AsJPEG().Save(filepath, false);
                    formsApp.ShowCapturedImage(filepath);
                });
                app.KeyWindow.RootViewController.DismissViewController(true, null);
            };

            imagePicker.Canceled += (sender, e) =>
            {
                formsApp.ShowCapturedImage(null);
                app.KeyWindow.RootViewController.DismissViewController(true, null);
            };
        }

        public void ChoosePhotoAsync(App app)
        {
            app.ShouldTakePicture += async () =>
            {
                try
                {
                    var mediaPicker = new MediaPicker();
                    var mediaFile = await mediaPicker.PickPhotoAsync();

                    app.ShowCapturedImage(mediaFile.Path);
                }
                catch (TaskCanceledException)
                {
                    app.ShowCapturedImage(null);
                }
            };
        }

        public static async Task RegisterWithMobilePushNotifications()
        {
            if (null != DeviceToken && IsAfterLogin)
            {
                // Register for push with Mobile Services
                //IEnumerable<string> tag = new List<string>() { "uniqueTag" };

                const string templateBodyAPNS = "{\"aps\":{\"alert\":\"$(messageParam)\"}}";

                //JObject templateBody = new JObject();
                //templateBody["body"] = notificationTemplate;

                //JObject templates = new JObject();
                //templates["ContosoMomentsApnsTemplate"] = templateBody;

                JObject templates = new JObject();
                templates["genericMessage"] = new JObject
                {
                    {"body", templateBodyAPNS}
                };

                //var expiryDate = DateTime.Now.AddDays(90).ToString
                //    (System.Globalization.CultureInfo.CreateSpecificCulture("en-US"));

                // Get Mobile Services client
                MobileServiceClient client = App.MobileService;
                var push = client.GetPush();

                try
                {
                    await push.RegisterAsync(DeviceToken, templates);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("RegisterWithMobilePushNotifications: " + ex.Message);
                }
            }
        }

        public override async void RegisteredForRemoteNotifications(UIApplication application, NSData deviceToken)
        {
            DeviceToken = deviceToken;

            if (IsAfterLogin)
                await RegisterWithMobilePushNotifications();
        }

        public override void ReceivedRemoteNotification(UIApplication application, NSDictionary userInfo)
        {
            NSObject inAppMessage;

            bool success = userInfo.TryGetValue(new NSString("inAppMessage"), out inAppMessage);

            if (success)
            {
                var alert = new UIAlertView("Got push notification", inAppMessage.ToString(), null, "OK", null);
                alert.Show();
            }
        }
    }
}
