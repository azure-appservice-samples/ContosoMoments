using System;
using System.Collections.Generic;
using System.Linq;

using Foundation;
using UIKit;
using System.IO;

namespace ContosoMoments.iOS
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the 
    // User Interface of the application, as well as listening (and optionally responding) to 
    // application events from iOS.
    [Register("AppDelegate")]
    public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
    {
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

            LoadApplication(new ContosoMoments.App());

//#error COMMENT WHEN DEBUGGING ON EMULATOR!
//            var imagePicker = new UIImagePickerController { SourceType = UIImagePickerControllerSourceType.Camera };
//            (Xamarin.Forms.Application.Current as App).ShouldTakePicture += () =>
//                app.KeyWindow.RootViewController.PresentViewController(imagePicker, true, null);

//            imagePicker.FinishedPickingMedia += (sender, e) =>
//            {
//                var filepath = Path.Combine(Environment.GetFolderPath(
//                                   Environment.SpecialFolder.MyDocuments), "tmp.png");
//                var image = (UIImage)e.Info.ObjectForKey(new NSString("UIImagePickerControllerOriginalImage"));
//                InvokeOnMainThread(() =>
//                {
//                    image.AsJPEG().Save(filepath, false);
//                    (Xamarin.Forms.Application.Current as App).ShowCapturedImage(filepath);
//                });
//                app.KeyWindow.RootViewController.DismissViewController(true, null);
//            };

//            imagePicker.Canceled += (sender, e) => app.KeyWindow.RootViewController.DismissViewController(true, null);

            return base.FinishedLaunching(app, options);
        }
    }
}
