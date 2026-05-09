using Avalonia;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using DownKyi.ViewModels.DownloadManager;

namespace DownKyi.Views.DownloadManager;

public partial class ViewDownloadFinished : UserControl
{
    public ViewDownloadFinished()
    {
        InitializeComponent();
    }

    private void Item_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is not Grid grid)
        {
            return;
        }

        var listBoxItem = grid.TemplatedParent as ListBoxItem;
        if (listBoxItem?.DataContext is not DownloadedItem item)
        {
            return;
        }

        if (DataContext is not ViewDownloadFinishedViewModel viewModel)
        {
            return;
        }

        viewModel.HandleItemClick(item, e.KeyModifiers);
        e.Handled = true;
    }

    private void ListBox_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        e.Handled = true;
    }
}
