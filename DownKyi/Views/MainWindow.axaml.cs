using Avalonia;
using Avalonia.Controls;
using DownKyi.Core.Settings;
using DownKyi.Core.Settings.Models;

namespace DownKyi.Views;

public partial class MainWindow : Window
{
    private WindowSettings _windowSettings;

    public MainWindow()
    {
        InitializeComponent();
        _windowSettings = SettingsManager.GetInstance().GetWindowSettings().Clone();
        ApplyWindowSettings();
    }

    private void ApplyWindowSettings()
    {
        Width = _windowSettings.Width;
        Height = _windowSettings.Height;
        if (double.IsNaN(_windowSettings.X) || double.IsNaN(_windowSettings.Y))
        {
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
        }
        else
        {
            Position = new PixelPoint((int)_windowSettings.X, (int)_windowSettings.Y);
        }
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        base.OnClosing(e);
        if (WindowState == WindowState.Normal)
        {
            _windowSettings.Width = Width;
            _windowSettings.Height = Height;
            _windowSettings.X = Position.X;
            _windowSettings.Y = Position.Y;
        }

        SettingsManager.GetInstance().SettingWindowSettings(_windowSettings);
    }

    // protected override void OnClosed(EventArgs e)
    // {
    //     base.OnClosed(e);
    //
    //     // 获取当前窗口的大小和位置
    //     _windowSettings.Width = Width;
    //     _windowSettings.Height = Height;
    //     _windowSettings.X = Position.X;
    //     _windowSettings.Y = Position.Y;
    //
    //     SettingsManager.GetInstance().SettingWindowSettings(_windowSettings);
    // }
}