using System;

using Foundation;
using UIKit;
using Microsoft.WindowsAzure.MobileServices;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

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
            LoadApplication(formsApp);
        
            return base.FinishedLaunching(app, options);
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
                MobileServiceClient client = App.Instance.MobileService;
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
