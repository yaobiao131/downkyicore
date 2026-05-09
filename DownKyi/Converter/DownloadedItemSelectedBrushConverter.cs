using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace DownKyi.Converter;

public class DownloadedItemSelectedBrushConverter : IValueConverter
{
    private static readonly IBrush SelectedBrush = new SolidColorBrush(Color.Parse("#D9ECFF"));
    private static readonly IBrush NormalBrush = Brushes.Transparent;

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is true ? SelectedBrush : NormalBrush;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
