using System;
using System.Globalization;
using Xamarin.Forms;

namespace SharpCooking.Converters
{
    public class GreaterThanOrEqualToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var parsedValue = (int)value;
            var parsedParameter = (int)parameter;

            return parsedValue >= parsedParameter;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}