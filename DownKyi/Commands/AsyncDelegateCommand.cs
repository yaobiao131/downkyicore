using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DownKyi.Commands;

public class AsyncDelegateCommand<T> : ICommand
{
    private readonly Func<T?, Task> _execute;
    private readonly Func<T, bool>? _canExecute;
    private bool _isExecuting;

    public event EventHandler? CanExecuteChanged;

    public AsyncDelegateCommand(Func<T?, Task> execute, Func<T, bool>? canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }
    
    public bool CanExecute(object? parameter)
    {
        if (parameter is null && typeof(T) == typeof(object))
        {
            return !_isExecuting && (_canExecute?.Invoke(default!) ?? true);
        }
        
        if (parameter is not T typedParameter)
        {
            return false;
        }
        return !_isExecuting && (_canExecute?.Invoke(typedParameter) ?? true);
    }

    public async void Execute(object? parameter)
    {
        if (parameter is null && typeof(T) == typeof(object))
        {
            _isExecuting = true;
            RaiseCanExecuteChanged();

            try
            {
                await _execute(default!);
            }
            finally
            {
                _isExecuting = false;
                RaiseCanExecuteChanged();
            }
            return;
        }
        
        if (parameter is not T typedParameter)
        {
            return;
        }
        
        _isExecuting = true;
        RaiseCanExecuteChanged();

        try
        {
            await _execute(typedParameter);
        }
        finally
        {
            _isExecuting = false;
            RaiseCanExecuteChanged();
        }
    }

    protected void RaiseCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}

public class AsyncDelegateCommand : AsyncDelegateCommand<object>
{
    public AsyncDelegateCommand(Func<object?, Task> execute, Func<object, bool>? canExecute = null)
        : base(execute, canExecute)
    {
    }
    
    public AsyncDelegateCommand(Func<Task> execute, Func<bool>? canExecute = null)
        : this(_ => execute(), 
            canExecute != null ? _ => canExecute() : null)
    {
    }
}