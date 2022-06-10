using System;
using System.Globalization;
using System.Windows.Data;

namespace Geomapmaker._helpers
{
    /// <summary>
    /// Value converter for concealed bool
    /// </summary>
    public class ConcealedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? "Y" : "N";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
