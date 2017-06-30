using MirrorConfigClient.ViewModels;
using System;
using System.Globalization;
using System.Windows.Data;

namespace MirrorConfigClient.ValueConverter
{
    public class BusinessObjectToViewModelConverter<T> : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is T)
                return new ViewModel<T>((T)value);
            else
                return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ViewModel<T>)
                return ((ViewModel<T>)value).BusinessObject;
            else
                return null;
        }
    }
}
