using System;
using System.ComponentModel;
using Avalonia.Controls;
using Prism.Commands;

namespace DownKyi.CustomControl;

public class CustomPagerViewModel : INotifyPropertyChanged
{
    public CustomPagerViewModel(int current, int count)
    {
        Current = current;
        Count = count;

        SetView();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    // Current修改的回调
    public delegate bool CurrentChangedHandler(int old, int current);

    public event CurrentChangedHandler? CurrentChanged;

    protected virtual bool OnCurrentChanged(int old, int current)
    {
        return CurrentChanged != null && CurrentChanged.Invoke(old, current);
    }

    // Count修改的回调
    public delegate void CountChangedHandler(int count);

    public event CountChangedHandler? CountChanged;

    protected virtual void OnCountChanged(int count)
    {
        CountChanged?.Invoke(count);
    }

    #region 绑定属性

    private bool? _visibility;

    public bool? Visibility
    {
        get => _visibility;
        set
        {
            _visibility = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Visibility"));
        }
    }

    private int _count;

    public int Count
    {
        get => _count;
        set
        {
            if (value < Current || value < 0)
            {
                Visibility = false;
                //throw new Exception("数值不在允许的范围内。");
                Console.WriteLine(value.ToString());
            }
            else
            {
                _count = value;

                Visibility = _count > 1;

                OnCountChanged(_count);

                SetView();

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Count"));
            }
        }
    }
    
    
    

    private int _current;

    public int Current
    {
        get
        {
            if (_current < 1)
            {
                _current = 1;
            }

            return _current;
        }
        set
        {
            if (Count > 0 && (value > Count || value < 1))
            {
                //throw new Exception("数值不在允许的范围内。");
            }
            else
            {
                var isSuccess = OnCurrentChanged(_current, value);
                if (isSuccess)
                {
                    _current = value;

                    SetView();

                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Current"));
                }
            }
        }
    }

    private int _first;

    public int First
    {
        get => _first;
        set
        {
            _first = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("First"));
        }
    }

    private int _previousSecond;

    public int PreviousSecond
    {
        get => _previousSecond;
        set
        {
            _previousSecond = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("PreviousSecond"));
        }
    }

    private int _previousFirst;

    public int PreviousFirst
    {
        get => _previousFirst;
        set
        {
            _previousFirst = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("PreviousFirst"));
        }
    }

    private int _nextFirst;

    public int NextFirst
    {
        get => _nextFirst;
        set
        {
            _nextFirst = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("NextFirst"));
        }
    }

    private int _nextSecond;

    public int NextSecond
    {
        get => _nextSecond;
        set
        {
            _nextSecond = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("NextSecond"));
        }
    }

    // 控制Current左边的控件
    private bool _previousVisibility;

    public bool PreviousVisibility
    {
        get => _previousVisibility;
        set
        {
            _previousVisibility = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("PreviousVisibility"));
        }
    }

    private bool _firstVisibility;

    public bool FirstVisibility
    {
        get => _firstVisibility;
        set
        {
            _firstVisibility = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("FirstVisibility"));
        }
    }

    private bool _leftJumpVisibility;

    public bool LeftJumpVisibility
    {
        get => _leftJumpVisibility;
        set
        {
            _leftJumpVisibility = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("LeftJumpVisibility"));
        }
    }

    private bool _previousSecondVisibility;

    public bool PreviousSecondVisibility
    {
        get => _previousSecondVisibility;
        set
        {
            _previousSecondVisibility = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("PreviousSecondVisibility"));
        }
    }

    private bool _previousFirstVisibility;

    public bool PreviousFirstVisibility
    {
        get => _previousFirstVisibility;
        set
        {
            _previousFirstVisibility = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("PreviousFirstVisibility"));
        }
    }

    // 控制Current右边的控件
    private bool _nextFirstVisibility;

    public bool NextFirstVisibility
    {
        get => _nextFirstVisibility;
        set
        {
            _nextFirstVisibility = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("NextFirstVisibility"));
        }
    }

    private bool _nextSecondVisibility;

    public bool NextSecondVisibility
    {
        get => _nextSecondVisibility;
        set
        {
            _nextSecondVisibility = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("NextSecondVisibility"));
        }
    }

    private bool _rightJumpVisibility;

    public bool RightJumpVisibility
    {
        get => _rightJumpVisibility;
        set
        {
            _rightJumpVisibility = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("RightJumpVisibility"));
        }
    }

    private bool _lastVisibility;

    public bool LastVisibility
    {
        get => _lastVisibility;
        set
        {
            _lastVisibility = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("LastVisibility"));
        }
    }

    private bool _nextVisibility;

    public bool NextVisibility
    {
        get => _nextVisibility;
        set
        {
            _nextVisibility = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("NextVisibility"));
        }
    }

    #endregion


    private DelegateCommand<object>? _previousCommand;

    public DelegateCommand<object> PreviousCommand => _previousCommand ??= new DelegateCommand<object>(PreviousExecuted);

    private void PreviousExecuted(object obj)
    {
        Current -= 1;

        SetView();
    }

    private DelegateCommand<object>? _firstCommand;

    public DelegateCommand<object> FirstCommand => _firstCommand ??= new DelegateCommand<object>(FirstExecuted);

    private void FirstExecuted(object obj)
    {
        Current = 1;

        SetView();
    }

    private DelegateCommand<object>? _previousSecondCommand;

    public DelegateCommand<object> PreviousSecondCommand => _previousSecondCommand ??= new DelegateCommand<object>(
        PreviousSecondExecuted);

    private void PreviousSecondExecuted(object obj)
    {
        Current -= 2;

        SetView();
    }

    private DelegateCommand<object>? _previousFirstCommand;

    public DelegateCommand<object> PreviousFirstCommand => _previousFirstCommand ??= new DelegateCommand<object>(PreviousFirstExecuted);

    private void PreviousFirstExecuted(object obj)
    {
        Current -= 1;

        SetView();
    }

    private DelegateCommand<object>? _nextFirstCommand;

    public DelegateCommand<object> NextFirstCommand => _nextFirstCommand ??= new DelegateCommand<object>(NextFirstExecuted);

    private void NextFirstExecuted(object obj)
    {
        Current += 1;

        SetView();
    }

    private DelegateCommand<object>? _nextSecondCommand;

    public DelegateCommand<object> NextSecondCommand => _nextSecondCommand ??= new DelegateCommand<object>(NextSecondExecuted);

    private void NextSecondExecuted(object obj)
    {
        Current += 2;

        SetView();
    }

    private DelegateCommand<object>? _lastCommand;

    public DelegateCommand<object> LastCommand => _lastCommand ??= new DelegateCommand<object>(LastExecuted);

    private void LastExecuted(object obj)
    {
        Current = Count;

        SetView();
    }

    private DelegateCommand<object>? _nextCommand;

    public DelegateCommand<object> NextCommand => _nextCommand ??= new DelegateCommand<object>(NextExecuted);

    private void NextExecuted(object obj)
    {
        Current += 1;

        SetView();
    }
    
    private DelegateCommand<object>? _jumpCommand;

    public DelegateCommand<object> JumpCommand => _jumpCommand ??= new DelegateCommand<object>(JumpExecuted);
    
    private void JumpExecuted(object obj)
    {
        if (obj is string s && int.TryParse(s,out var i))
        {
            Current = (i >= _count) ? _count : i;
            SetView();
        }
    }
    
    /// <summary>
    /// 控制显示，暴力实现，以后重构
    /// </summary>
    private void SetView()
    {
        First = 1;
        PreviousSecond = Current - 2;
        PreviousFirst = Current - 1;
        NextFirst = Current + 1;
        NextSecond = Current + 2;

        // 控制Current左边的控件
        if (Current == 1)
        {
            PreviousVisibility = false;
            FirstVisibility = false;
            LeftJumpVisibility = false;
            PreviousSecondVisibility = false;
            PreviousFirstVisibility = false;
        }
        else if (Current == 2)
        {
            PreviousVisibility = true;
            FirstVisibility = false;
            LeftJumpVisibility = false;
            PreviousSecondVisibility = false;
            PreviousFirstVisibility = true;
        }
        else if (Current == 3)
        {
            PreviousVisibility = true;
            FirstVisibility = false;
            LeftJumpVisibility = false;
            PreviousSecondVisibility = true;
            PreviousFirstVisibility = true;
        }
        else if (Current == 4)
        {
            PreviousVisibility = true;
            FirstVisibility = true;
            LeftJumpVisibility = false;
            PreviousSecondVisibility = true;
            PreviousFirstVisibility = true;
        }
        else
        {
            PreviousVisibility = true;
            FirstVisibility = true;
            LeftJumpVisibility = true;
            PreviousSecondVisibility = true;
            PreviousFirstVisibility = true;
        }

        // 控制Current右边的控件
        if (Current == Count)
        {
            NextFirstVisibility = false;
            NextSecondVisibility = false;
            RightJumpVisibility = false;
            LastVisibility = false;
            NextVisibility = false;
        }
        else if (Current == Count - 1)
        {
            NextFirstVisibility = true;
            NextSecondVisibility = false;
            RightJumpVisibility = false;
            LastVisibility = false;
            NextVisibility = true;
        }
        else if (Current == Count - 2)
        {
            NextFirstVisibility = true;
            NextSecondVisibility = true;
            RightJumpVisibility = false;
            LastVisibility = false;
            NextVisibility = true;
        }
        else if (Current == Count - 3)
        {
            NextFirstVisibility = true;
            NextSecondVisibility = true;
            RightJumpVisibility = false;
            LastVisibility = true;
            NextVisibility = true;
        }
        else
        {
            NextFirstVisibility = true;
            NextSecondVisibility = true;
            RightJumpVisibility = true;
            LastVisibility = true;
            NextVisibility = true;
        }
    }
}