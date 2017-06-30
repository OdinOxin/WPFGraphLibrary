using System;
using System.Globalization;
using System.Windows.Data;

namespace MirrorConfigClient.ValueConverter
{
    public class ObjectAvaiableConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new InvalidOperationException("Cannot convert from bool to object!");
        }
    }
}
