using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Xaml.Interactivity;

namespace DownKyi.CustomAction;
public class ResetGridSplitterBehavior : Behavior<GridSplitter>
{
    private Dictionary<int, GridLength> _originalColumnWidths = new ();
    private Dictionary<int, GridLength> _originalRowHeights = new ();
    private Grid _parentGrid;
  
    protected override void OnAttached()
    {
        base.OnAttached();
        var gridSplitter = AssociatedObject;
        _parentGrid = gridSplitter.Parent as Grid;

        if (_parentGrid != null)
        {
            for (int i = 0; i < _parentGrid.ColumnDefinitions.Count; i++)
            {
                _originalColumnWidths[i] = _parentGrid.ColumnDefinitions[i].Width;
            }

            for (int i = 0; i < _parentGrid.RowDefinitions.Count; i++)
            {
                _originalRowHeights[i] = _parentGrid.RowDefinitions[i].Height;
            }
        }
     
    }
    
    private void OnRefreshRequested(object sender, EventArgs e)
    {
        ResetGrid();
    }
    
    public void ResetGrid()
    {
        if (_parentGrid != null)
        {
            foreach (var kvp in _originalColumnWidths)
            {
                _parentGrid.ColumnDefinitions[kvp.Key].Width = kvp.Value;
            }

            foreach (var kvp in _originalRowHeights)
            {
                _parentGrid.RowDefinitions[kvp.Key].Height = kvp.Value;
            }
        }
    }

    protected override void OnDetachedFromVisualTree()
    {
        base.OnDetachedFromVisualTree();
        ResetGrid();
    }

    
}
