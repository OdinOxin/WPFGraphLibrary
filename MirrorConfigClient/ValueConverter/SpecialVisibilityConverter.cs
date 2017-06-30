using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MirrorConfigClient.ValueConverter
{
    public class SpecialVisibilityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            foreach (var val in values)
                if (!(val is bool))
                    return Visibility.Collapsed;
            if (((bool)values[0] || (bool)values[1]) && (bool)values[2])
                return Visibility.Visible;
            return Visibility.Collapsed;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new InvalidOperationException();
        }
    }
}
