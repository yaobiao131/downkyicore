using System;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace DownKyi.Converter;


public class BoolToBrushConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var isSelected = value is true;
        var param = parameter?.ToString();

        if (Application.Current?.Styles.TryGetResource(
            isSelected
                ? GetSelectedResourceKey(param)
                : GetUnselectedResourceKey(param), Application.Current.ActualThemeVariant, out var resource) == true)
        {
            return resource;
        }

        return isSelected ? Brushes.DodgerBlue : Brushes.Transparent;
    }

    private static string GetSelectedResourceKey(string? param)
    {
        return param switch
        {
            "SelectedForeground" => "BrushPrimary",
            _ => "BrushPrimaryTransparent"
        };
    }

    private static string GetUnselectedResourceKey(string? param)
    {
        return param switch
        {
            "SelectedForeground" => "BrushForeground",
            _ => "BrushBackground"
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
