using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Xamarin.Forms;

namespace ContosoMoments.Converters
{
    public class ImagePathForSizeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string param = (string)parameter;
            IDictionary<string, Uri> imagePaths = (IDictionary<string, Uri>)value;
            UriImageSource retVal = null;

            if (null != imagePaths)
            {
                if (imagePaths.ContainsKey(param))
                {
                    return new UriImageSource() { Uri = imagePaths[param], CachingEnabled = false};
                }
            }

            return retVal;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
