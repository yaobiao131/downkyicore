using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
namespace DownKyi.ViewModels;

public sealed class ImmutableObservableCollection<T> : IList<T>,IList, INotifyCollectionChanged, INotifyPropertyChanged
{
    private ImmutableList<T> _items;

    public ImmutableObservableCollection()
    {
        _items = ImmutableList<T>.Empty;
    }

    public ImmutableObservableCollection(IEnumerable<T> items)
    {
        _items = ImmutableList<T>.Empty.AddRange(items);
    }
    
    public T this[int index]
    {
        get => _items[index];
        set
        {
            var oldItem = _items[index];
            _items = _items.SetItem(index, value);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(
                NotifyCollectionChangedAction.Replace, value, oldItem, index));
        }
    }

    public int IndexOf(T item) => _items.IndexOf(item);

    public void Insert(int index, T item)
    {
        _items = _items.Insert(index, item);
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(
            NotifyCollectionChangedAction.Add, item, index));
        OnPropertyChanged(nameof(Count));
    }

    public void Remove(object? value)
    {
        throw new NotImplementedException();
    }

    public void RemoveAt(int index)
    {
        var removedItem = _items[index];
        _items = _items.RemoveAt(index);
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(
            NotifyCollectionChangedAction.Remove, removedItem, index));
        OnPropertyChanged(nameof(Count));
    }

    public bool IsFixedSize => false;
    
    public void Add(T item)
    {
        CheckReentrancy();
        _items = _items.Add(item);
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(
            NotifyCollectionChangedAction.Add, item, _items.Count - 1));
        OnPropertyChanged(nameof(Count));
    }

    public bool Remove(T item)
    {
        CheckReentrancy();
        var index = _items.IndexOf(item);
        if (index < 0) return false;

        _items = _items.Remove(item);
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(
            NotifyCollectionChangedAction.Remove, item, index));
        OnPropertyChanged(nameof(Count));
        return true;
    }

    public int Add(object? value)
    {
#nullable disable
        T obj;
        try
        {
            obj = (T) value;
        }
        catch (InvalidCastException ex)
        {
            throw new ArgumentException(
                $"Value cannot be cast to type {typeof(T).Name}.", 
                nameof(value), ex);
        }
        this.Add(obj);
        return this.Count - 1;
    }

#nullable restore

    public void Clear()
    {
        if (_items.IsEmpty) return;
        CheckReentrancy();
        _items = ImmutableList<T>.Empty;
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(
            NotifyCollectionChangedAction.Reset));
        OnPropertyChanged(nameof(Count));
    }

    public bool Contains(object? value)
    {
        throw new NotImplementedException();
    }

    public int IndexOf(object? value)
    {
        throw new NotImplementedException();
    }

    public void Insert(int index, object? value)
    {
        throw new NotImplementedException();
    }

    public bool Contains(T item) => _items.Contains(item);
    public void CopyTo(T[] array, int arrayIndex) => _items.CopyTo(array, arrayIndex);
    public void CopyTo(Array array, int index)
    {
        throw new NotImplementedException();
    }

    public int Count => _items.Count;
    public bool IsSynchronized => false;
    public object SyncRoot => this;
    public bool IsReadOnly => false;
    object? IList.this[int index]
    {
        get => this[index];
        set
        {
            #nullable disable
            T obj;
            try
            {
                obj = (T) value;
            }
            catch (InvalidCastException ex)
            {
                throw new ArgumentException(
                    $"Value cannot be cast to type {typeof(T).Name}.", 
                    nameof(value), ex);
            }
            this[index] = obj;
            #nullable restore
        }
    }


    public IEnumerator<T> GetEnumerator() => _items.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => _items.GetEnumerator();
    
    public event NotifyCollectionChangedEventHandler? CollectionChanged;
    public event PropertyChangedEventHandler? PropertyChanged;

    private int _blockReentrancyCount;

    private void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
    {
        NotifyCollectionChangedEventHandler? collectionChanged = this.CollectionChanged;
        if (collectionChanged == null)
            return;
    
        ++this._blockReentrancyCount;
        try
        {
            collectionChanged(this, e);
        }
        finally
        {
            --this._blockReentrancyCount;
        }
    }


    private void CheckReentrancy()
    {
        if (this._blockReentrancyCount <= 0)
            return; 
        NotifyCollectionChangedEventHandler? collectionChanged = this.CollectionChanged;
        if (collectionChanged != null && !HasSingleTarget(collectionChanged))
        {
            throw new InvalidOperationException("ObservableCollectionReentrancyNotAllowed");
        }
    }
    
    private bool HasSingleTarget(NotifyCollectionChangedEventHandler? handler)
    {
        if (handler == null)
            return true;
        
        return handler.GetInvocationList().Length <= 1;
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public void AddRange(List<T> downloadingItems)
    {
       _items = _items.AddRange(downloadingItems);
    }
}