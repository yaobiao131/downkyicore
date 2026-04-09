using Prism.Mvvm;
using Prism.Regions;

namespace DownKyi.Models;


public class TabItemModel : BindableBase
{
    private string _id = string.Empty;
    private string _title = string.Empty;
    private string _viewName = string.Empty;
    private NavigationParameters _parameters = new();
    private bool _canClose = true;
    private bool _isHome;

   
    public string Id
    {
        get => _id;
        set => SetProperty(ref _id, value);
    }
    
    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }
    
    public string ViewName
    {
        get => _viewName;
        set => SetProperty(ref _viewName, value);
    }
    
    public NavigationParameters Parameters
    {
        get => _parameters;
        set => SetProperty(ref _parameters, value);
    }
    
    public bool CanClose
    {
        get => _canClose;
        set => SetProperty(ref _canClose, value);
    }
    
    public bool IsHome
    {
        get => _isHome;
        set => SetProperty(ref _isHome, value);
    }
}
