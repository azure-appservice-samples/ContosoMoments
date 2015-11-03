using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;

namespace ContosoMoments.WinPhone
{
	public partial class MainPage : global::Xamarin.Forms.Platform.WinPhone.FormsApplicationPage
	{
		public MainPage ()
		{
			InitializeComponent ();
			SupportedOrientations = SupportedPageOrientation.PortraitOrLandscape;

			global::Xamarin.Forms.Forms.Init ();

           
            LoadApplication(new ContosoMoments.App ());

            ContosoMoments.App.Instance.ShouldTakePicture += () => {
                CameraCaptureTask cameraCaptureTask = new CameraCaptureTask();
                cameraCaptureTask.Completed += CameraCaptureTaskOnCompleted;

                cameraCaptureTask.Show();
            };
        }

        private void CameraCaptureTaskOnCompleted(object sender, PhotoResult e)
        {
            if (e.TaskResult == TaskResult.None)
            {
                return;
            }

            if (e.TaskResult == TaskResult.Cancel)
            {
                return;
            }

            ContosoMoments.App.Instance.ShowCapturedImage(e.ChosenPhoto);
        }
    }
}
