using System;
using System.ComponentModel;
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

    public event PropertyChangedEventHandler PropertyChanged;

    // Current修改的回调
    public delegate bool CurrentChangedHandler(int old, int current);

    public event CurrentChangedHandler CurrentChanged;

    protected virtual bool OnCurrentChanged(int old, int current)
    {
        if (CurrentChanged == null)
        {
            return false;
        }
        else
        {
            return CurrentChanged.Invoke(old, current);
        }
    }

    // Count修改的回调
    public delegate void CountChangedHandler(int count);

    public event CountChangedHandler CountChanged;

    protected virtual void OnCountChanged(int count)
    {
        CountChanged?.Invoke(count);
    }

    #region 绑定属性

    private bool visibility;

    public bool Visibility
    {
        get { return visibility; }
        set
        {
            visibility = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Visibility"));
        }
    }

    private int count;

    public int Count
    {
        get { return count; }
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
                count = value;

                if (count <= 1)
                {
                    Visibility = false;
                }
                else
                {
                    Visibility = true;
                }

                OnCountChanged(count);

                SetView();

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Count"));
            }
        }
    }

    private int current;

    public int Current
    {
        get
        {
            if (current < 1)
            {
                current = 1;
            }

            return current;
        }
        set
        {
            if (Count > 0 && (value > Count || value < 1))
            {
                //throw new Exception("数值不在允许的范围内。");
            }
            else
            {
                bool isSuccess = OnCurrentChanged(current, value);
                if (isSuccess)
                {
                    current = value;

                    SetView();

                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Current"));
                }
            }
        }
    }

    private int first;

    public int First
    {
        get { return first; }
        set
        {
            first = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("First"));
        }
    }

    private int previousSecond;

    public int PreviousSecond
    {
        get { return previousSecond; }
        set
        {
            previousSecond = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("PreviousSecond"));
        }
    }

    private int previousFirst;

    public int PreviousFirst
    {
        get { return previousFirst; }
        set
        {
            previousFirst = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("PreviousFirst"));
        }
    }

    private int nextFirst;

    public int NextFirst
    {
        get { return nextFirst; }
        set
        {
            nextFirst = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("NextFirst"));
        }
    }

    private int nextSecond;

    public int NextSecond
    {
        get { return nextSecond; }
        set
        {
            nextSecond = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("NextSecond"));
        }
    }

    // 控制Current左边的控件
    private bool previousVisibility;

    public bool PreviousVisibility
    {
        get { return previousVisibility; }
        set
        {
            previousVisibility = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("PreviousVisibility"));
        }
    }

    private bool firstVisibility;

    public bool FirstVisibility
    {
        get { return firstVisibility; }
        set
        {
            firstVisibility = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("FirstVisibility"));
        }
    }

    private bool leftJumpVisibility;

    public bool LeftJumpVisibility
    {
        get { return leftJumpVisibility; }
        set
        {
            leftJumpVisibility = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("LeftJumpVisibility"));
        }
    }

    private bool previousSecondVisibility;

    public bool PreviousSecondVisibility
    {
        get { return previousSecondVisibility; }
        set
        {
            previousSecondVisibility = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("PreviousSecondVisibility"));
        }
    }

    private bool previousFirstVisibility;

    public bool PreviousFirstVisibility
    {
        get { return previousFirstVisibility; }
        set
        {
            previousFirstVisibility = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("PreviousFirstVisibility"));
        }
    }

    // 控制Current右边的控件
    private bool nextFirstVisibility;

    public bool NextFirstVisibility
    {
        get { return nextFirstVisibility; }
        set
        {
            nextFirstVisibility = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("NextFirstVisibility"));
        }
    }

    private bool nextSecondVisibility;

    public bool NextSecondVisibility
    {
        get { return nextSecondVisibility; }
        set
        {
            nextSecondVisibility = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("NextSecondVisibility"));
        }
    }

    private bool rightJumpVisibility;

    public bool RightJumpVisibility
    {
        get { return rightJumpVisibility; }
        set
        {
            rightJumpVisibility = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("RightJumpVisibility"));
        }
    }

    private bool lastVisibility;

    public bool LastVisibility
    {
        get { return lastVisibility; }
        set
        {
            lastVisibility = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("LastVisibility"));
        }
    }

    private bool nextVisibility;

    public bool NextVisibility
    {
        get { return nextVisibility; }
        set
        {
            nextVisibility = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("NextVisibility"));
        }
    }

    #endregion


    private DelegateCommand<object> previousCommand;

    public DelegateCommand<object> PreviousCommand =>
        previousCommand ?? (previousCommand = new DelegateCommand<object>(PreviousExecuted));

    public void PreviousExecuted(object obj)
    {
        Current -= 1;

        SetView();
    }

    private DelegateCommand<object> firstCommand;

    public DelegateCommand<object> FirstCommand =>
        firstCommand ?? (firstCommand = new DelegateCommand<object>(FirstExecuted));

    public void FirstExecuted(object obj)
    {
        Current = 1;

        SetView();
    }

    private DelegateCommand<object> previousSecondCommand;

    public DelegateCommand<object> PreviousSecondCommand => previousSecondCommand ??
                                                            (previousSecondCommand =
                                                                new DelegateCommand<object>(
                                                                    PreviousSecondExecuted));

    public void PreviousSecondExecuted(object obj)
    {
        Current -= 2;

        SetView();
    }

    private DelegateCommand<object> previousFirstCommand;

    public DelegateCommand<object> PreviousFirstCommand => previousFirstCommand ??
                                                           (previousFirstCommand =
                                                               new DelegateCommand<object>(PreviousFirstExecuted));

    public void PreviousFirstExecuted(object obj)
    {
        Current -= 1;

        SetView();
    }

    private DelegateCommand<object> nextFirstCommand;

    public DelegateCommand<object> NextFirstCommand =>
        nextFirstCommand ?? (nextFirstCommand = new DelegateCommand<object>(NextFirstExecuted));

    public void NextFirstExecuted(object obj)
    {
        Current += 1;

        SetView();
    }

    private DelegateCommand<object> nextSecondCommand;

    public DelegateCommand<object> NextSecondCommand => nextSecondCommand ??
                                                        (nextSecondCommand =
                                                            new DelegateCommand<object>(NextSecondExecuted));

    public void NextSecondExecuted(object obj)
    {
        Current += 2;

        SetView();
    }

    private DelegateCommand<object> lastCommand;

    public DelegateCommand<object> LastCommand =>
        lastCommand ?? (lastCommand = new DelegateCommand<object>(LastExecuted));

    public void LastExecuted(object obj)
    {
        Current = Count;

        SetView();
    }

    private DelegateCommand<object> nextCommand;

    public DelegateCommand<object> NextCommand =>
        nextCommand ?? (nextCommand = new DelegateCommand<object>(NextExecuted));

    public void NextExecuted(object obj)
    {
        Current += 1;

        SetView();
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