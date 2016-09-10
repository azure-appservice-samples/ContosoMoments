using System.ComponentModel;
using Android.Graphics;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using ContosoMoments.Droid;
using ContosoMoments.Views;

[assembly: ExportRenderer(typeof(CustomImage), typeof(CustomImageRenderer))]
namespace ContosoMoments.Droid
{
    // This custom render resizes bitmaps in memory before displaying them.
    // The Xamarin.Forms image control uses memory proportional to the image dimensions on Android,
    // so you will get an out of memory error with only a few images if they have large dimensions.
    // See https://developer.xamarin.com/recipes/android/resources/general/load_large_bitmaps_efficiently/

    public class CustomImageRenderer : ImageRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Image> e)
        {
            base.OnElementChanged(e);
        }

        private bool isScaled;

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            var customImage = (CustomImage)Element;

            if ((!isScaled || e.PropertyName == "ImageSource") && customImage.ImageSource?.Length > 0) {

                var options = GetBitmapOptionsOfImage(customImage.ImageSource);

                // resize to the element dimensions
                var width = (int)customImage.Width;
                var height = (int)customImage.Height;

                if (width < 0 || height < 0) { // the element has not fully rendered, so wait for another callback
                    return;
                }

                options.InSampleSize = CalculateInSampleSize(options, width, height);

                // Decode bitmap with inSampleSize set
                options.InJustDecodeBounds = false;

                var bitmap = BitmapFactory.DecodeFile(customImage.ImageSource, options);

                //Set the bitmap to the native control
                Control.SetImageBitmap(bitmap);

                isScaled = true;
            }

        }

        private static BitmapFactory.Options GetBitmapOptionsOfImage(string filename)
        {
            BitmapFactory.Options options = new BitmapFactory.Options {
                InJustDecodeBounds = true
            };

            // The result will be null because InJustDecodeBounds == true.
            Bitmap result = BitmapFactory.DecodeFile(filename, options);

            int imageHeight = options.OutHeight;
            int imageWidth = options.OutWidth;

            return options;
        }

        public static int CalculateInSampleSize(BitmapFactory.Options options, int reqWidth, int reqHeight)
        {
            // Raw height and width of image
            float height = options.OutHeight;
            float width = options.OutWidth;
            double inSampleSize = 1D;

            if (height > reqHeight || width > reqWidth) {
                int halfHeight = (int)(height / 2);
                int halfWidth = (int)(width / 2);

                // Calculate a inSampleSize that is a power of 2 - the decoder will use a value that is a power of two anyway.
                while ((halfHeight / inSampleSize) > reqHeight && (halfWidth / inSampleSize) > reqWidth) {
                    inSampleSize *= 2;
                }
            }

            return (int)inSampleSize;
        }
    }
}