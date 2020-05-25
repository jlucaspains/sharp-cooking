using System;
using System.Globalization;
using System.IO;
using Xamarin.Forms;

namespace SharpCooking.Converters
{
    public class ByteArrayToImageSourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is byte[] sourceArray)
            {
                var stream = new MemoryStream(sourceArray);

                return ImageSource.FromStream(() => stream);
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}