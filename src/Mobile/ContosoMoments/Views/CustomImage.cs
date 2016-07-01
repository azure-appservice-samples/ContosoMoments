using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Xamarin.Forms;

namespace ContosoMoments.Views
{
    public class CustomImage : Image
    {
        public static readonly BindableProperty ImageSourceProperty =
            BindableProperty.Create(
                "ImageSource", typeof(string),
                typeof(CustomImage), default(string), propertyChanged: OnImageSourcePropertyChanged);


        private static void OnImageSourcePropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (Device.OS != TargetPlatform.Android) {
                var image = (CustomImage)bindable; // use default behavior on non-Android platforms

                var baseImage = (Image)bindable;
                baseImage.Source = image.ImageSource;
            }
        }

        public string ImageSource
        {
            get { return GetValue(ImageSourceProperty) as string; }
            set { SetValue(ImageSourceProperty, value); }
        }
    }
}
