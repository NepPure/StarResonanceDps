using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace StarResonanceDpsAnalysis.WPF.Models;

/// <summary>
/// ObservableCollection with bulk operation support to minimize UI notifications
/// </summary>
/// <typeparam name="T">The type of elements in the collection</typeparam>
public class BulkObservableCollection<T> : ObservableCollection<T>
{
    private bool _isUpdating;

    /// <summary>
    /// Begins a bulk update operation. Notifications are suppressed until EndUpdate is called.
    /// </summary>
    public void BeginUpdate()
    {
        _isUpdating = true;
    }

    /// <summary>
    /// Ends a bulk update operation and raises a Reset notification.
    /// </summary>
    public void EndUpdate()
    {
        _isUpdating = false;
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    /// <summary>
    /// Adds multiple items to the collection in bulk
    /// </summary>
    /// <param name="items">Items to add</param>
    public void AddRange(IEnumerable<T> items)
    {
        BeginUpdate();
        try
        {
            foreach (var item in items)
            {
                Add(item);
            }
        }
        finally
        {
            EndUpdate();
        }
    }

    /// <summary>
    /// Replaces all items in the collection
    /// </summary>
    /// <param name="items">New items for the collection</param>
    public void ReplaceAll(IEnumerable<T> items)
    {
        BeginUpdate();
        try
        {
            Clear();
            foreach (var item in items)
            {
                Add(item);
            }
        }
        finally
        {
            EndUpdate();
        }
    }

    /// <summary>
    /// Sorts the collection in place using the provided comparison function
    /// </summary>
    /// <param name="comparison">Comparison function</param>
    public void Sort(Comparison<T> comparison)
    {
        var sortedList = Items.ToList();
        sortedList.Sort(comparison);
        
        BeginUpdate();
        try
        {
            Items.Clear();
            foreach (var item in sortedList)
            {
                Items.Add(item);
            }
        }
        finally
        {
            EndUpdate();
        }
    }

    /// <summary>
    /// Sorts the collection in place using IComparer
    /// </summary>
    /// <param name="comparer">Comparer to use for sorting</param>
    public void Sort(IComparer<T> comparer)
    {
        Sort(comparer.Compare);
    }

    /// <summary>
    /// Sorts the collection in place using a key selector
    /// </summary>
    /// <param name="keySelector">Function to extract the sort key</param>
    /// <param name="descending">Whether to sort in descending order</param>
    public void SortBy<TKey>(Func<T, TKey> keySelector, bool descending = false) where TKey : IComparable<TKey>
    {
        var sortedList = descending 
            ? Items.OrderByDescending(keySelector).ToList()
            : Items.OrderBy(keySelector).ToList();
        
        // BeginUpdate();
        try
        {
            for (var index = 0; index < sortedList.Count; index++)
            {
                var itm = sortedList[index];
                Items[index] = itm;
            }
            // Items.Clear();
            // foreach (var item in sortedList)
            // {
            //     Items.Add(item);
            // }
        }
        finally
        {
            // EndUpdate();
        }
    }

    protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
    {
        if (!_isUpdating)
        {
            base.OnCollectionChanged(e);
        }
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        if (!_isUpdating)
        {
            base.OnPropertyChanged(e);
        }
    }
}