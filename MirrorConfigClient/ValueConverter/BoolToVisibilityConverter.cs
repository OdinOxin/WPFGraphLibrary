using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MirrorConfigClient.ValueConverter
{
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool)
                return (bool)value ? Visibility.Visible : Visibility.Collapsed;
            throw new InvalidOperationException("Cannot convert " + value.GetType().ToString() + " to bool");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility)
                return (Visibility)value == Visibility.Visible;
            return false;
        }
    }
}
