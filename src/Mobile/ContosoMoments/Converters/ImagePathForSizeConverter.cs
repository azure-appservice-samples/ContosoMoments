using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Microsoft.WindowsAzure.MobileServices.Files;
using Microsoft.WindowsAzure.MobileServices.Files.Sync;
using Xamarin.Forms;

namespace ContosoMoments.Converters
{
    public class ImagePathForSizeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var param = (string)parameter;
            var image = (Models.Image)value;

            var toDownload = new MobileServiceFile(image.File.Id, image.File.Name, image.File.TableName, image.File.ParentId) {
                StoreUri = image.File.StoreUri.Replace("lg", param)
            };

            return toDownload;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
