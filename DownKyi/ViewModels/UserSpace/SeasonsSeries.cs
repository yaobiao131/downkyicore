using Avalonia.Media.Imaging;
using DownKyi.Images;
using Prism.Mvvm;

namespace DownKyi.ViewModels.UserSpace;

public class SeasonsSeries : BindableBase
{
    public long Id { get; set; }

    private string _cover;

    public string Cover
    {
        get => _cover;
        set => SetProperty(ref _cover, value);
    }

    private VectorImage typeImage;

    public VectorImage TypeImage
    {
        get => typeImage;
        set => SetProperty(ref typeImage, value);
    }

    private string name;

    public string Name
    {
        get => name;
        set => SetProperty(ref name, value);
    }

    private int count;

    public int Count
    {
        get => count;
        set => SetProperty(ref count, value);
    }

    private string ctime;

    public string Ctime
    {
        get => ctime;
        set => SetProperty(ref ctime, value);
    }
}