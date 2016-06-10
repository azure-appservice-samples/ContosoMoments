using Foundation;
using Microsoft.WindowsAzure.MobileServices;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;
using UIKit;

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

            if (UIDevice.CurrentDevice.CheckSystemVersion(8, 0)) {
                var settings = UIUserNotificationSettings.GetSettingsForTypes(UIUserNotificationType.Sound |
                                                                              UIUserNotificationType.Alert |
                                                                              UIUserNotificationType.Badge, null);

                UIApplication.SharedApplication.RegisterUserNotificationSettings(settings);
                UIApplication.SharedApplication.RegisterForRemoteNotifications();

            }
            else {
                UIApplication.SharedApplication.RegisterForRemoteNotificationTypes(UIRemoteNotificationType.Badge |
                                                                                   UIRemoteNotificationType.Sound |
                                                                                   UIRemoteNotificationType.Alert);
            }

            var formsApp = new ContosoMoments.App();
            LoadApplication(formsApp);

            Facebook.CoreKit.ApplicationDelegate.SharedInstance.FinishedLaunching(app, options);

            return base.FinishedLaunching(app, options);
        }


        public override bool OpenUrl(UIApplication application, NSUrl url, string sourceApplication, NSObject annotation)
        {
            // We need to handle URLs by passing them to their own OpenUrl in order to make the SSO authentication works.
            return Facebook.CoreKit.ApplicationDelegate.SharedInstance.OpenUrl(application, url, sourceApplication, annotation);
        }

        public static async Task RegisterWithMobilePushNotifications()
        {
            if (DeviceToken != null && IsAfterLogin) {

                var apnsBody = new JObject {
                    {
                        "aps",
                        new JObject {
                            { "alert", "$(messageParam)" }
                        }
                    }
                };

                var template = new JObject {
                    {
                        "genericMessage",
                        new JObject {
                            {"body", apnsBody}
                        }
                    }
                };

                try {
                    var push = App.Instance.MobileService.GetPush();
                    await push.RegisterAsync(DeviceToken, template);
                }
                catch (Exception ex) {
                    System.Diagnostics.Debug.WriteLine("Exception in RegisterWithMobilePushNotifications: " + ex.Message);
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

            if (success) {
                var alert = new UIAlertView("Got push notification", inAppMessage.ToString(), null, "OK", null);
                alert.Show();
            }
        }

        public override void OnActivated(UIApplication uiApplication)
        {
            base.OnActivated(uiApplication);

            // log app activation to Facebook app events
            Facebook.CoreKit.AppEvents.ActivateApp();
            Facebook.CoreKit.Settings.LimitEventAndDataUsage = true; // tell Facebook not to use app events for ad serving
        }
    }
}
