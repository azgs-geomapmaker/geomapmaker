using System;
using System.Globalization;
using System.Windows.Data;

namespace Geomapmaker._helpers
{

    /// <summary>
    /// Value converter for radio button groups
    /// </summary>
    public class RadioConfidenceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Equals(value, parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return parameter;
        }
    }

    /// <summary>
    /// Value converter for slider
    /// </summary>
    public class SliderConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch (value)
            {
                case "Low":
                    return 0;
                case "Medium":
                    return 1;
                case "High":
                    return 2;
                default:
                    return -1;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch (value)
            {
                case 0.0:
                    return "Low";
                case 1.0:
                    return "Medium";
                case 2.0:
                    return "High";
                default:
                    return "N/A";
            }
        }
    }

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
