using Prism.Mvvm;

namespace DownKyi.Images;

public class VectorImage : BindableBase
{
    private double _width;

    public double Width
    {
        get => _width;
        set => SetProperty(ref _width, value);
    }

    private double _height;

    public double Height
    {
        get => _height;
        set => SetProperty(ref _height, value);
    }

    private string? _data;

    public string? Data
    {
        get => _data;
        set => SetProperty(ref _data, value);
    }

    private string? _fill;

    public string? Fill
    {
        get => _fill;
        set => SetProperty(ref _fill, value);
    }
}