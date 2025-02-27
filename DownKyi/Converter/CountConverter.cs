using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace DownKyi.Converter
{
    public class CountConverter : IValueConverter
    {
        public int Count { get; set; }
        

        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            var i = (int?)value > Count;
            return (int?)value > Count;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
