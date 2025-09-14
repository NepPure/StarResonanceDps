using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using StarResonanceDpsAnalysis.WPF.Controls.Models;

namespace StarResonanceDpsAnalysis.WPF.Controls;

public class SortedDpsControl : Control
{
#if false
    // Backwards-compatible Data property (existing) — now with change callback
    public static readonly DependencyProperty DataProperty = DependencyProperty.Register(
        nameof(Data), typeof(IEnumerable), typeof(SortedDpsControl),
        new PropertyMetadata(null, OnDataChanged));

    // New ItemsSource property (preferred)
    public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
        nameof(ItemsSource), typeof(IEnumerable), typeof(SortedDpsControl),
        new PropertyMetadata(null, OnItemsSourceChanged));

    // ItemTemplate for customizing item visuals
    public static readonly DependencyProperty ItemTemplateProperty = DependencyProperty.Register(
        nameof(ItemTemplate), typeof(DataTemplate), typeof(SortedDpsControl), new PropertyMetadata(null));

    public static readonly DependencyProperty ItemHeightProperty = DependencyProperty.Register(
        nameof(ItemHeight), typeof(double), typeof(SortedDpsControl), new PropertyMetadata(30.0));

    // Selected item
    public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register(
        nameof(SelectedItem), typeof(ProgressBarData), typeof(SortedDpsControl), new PropertyMetadata(null));

    // Item click command
    public static readonly DependencyProperty ItemClickCommandProperty = DependencyProperty.Register(
        nameof(ItemClickCommand), typeof(ICommand), typeof(SortedDpsControl), new PropertyMetadata(null));

    // Sorting API
    public static readonly DependencyProperty SortMemberPathProperty = DependencyProperty.Register(
        nameof(SortMemberPath), typeof(string), typeof(SortedDpsControl),
        new PropertyMetadata(string.Empty, OnSortChanged));

    public static readonly DependencyProperty SortDirectionProperty = DependencyProperty.Register(
        nameof(SortDirection), typeof(ListSortDirection), typeof(SortedDpsControl),
        new PropertyMetadata(ListSortDirection.Descending, OnSortChanged));

    private readonly Dictionary<INotifyPropertyChanged, PropertyChangedEventHandler> _itemSubscriptions = new();
    private INotifyCollectionChanged? _currentCollectionNotify;
    private ICollectionView? _currentView;

    private ListBox? _listBox;

    static SortedDpsControl()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(SortedDpsControl),
            new FrameworkPropertyMetadata(typeof(SortedDpsControl)));
    }

    public IEnumerable? Data
    {
        get => (IEnumerable?)GetValue(DataProperty);
        set => SetValue(DataProperty, value);
    }

    public IEnumerable? ItemsSource
    {
        get => (IEnumerable?)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public DataTemplate? ItemTemplate
    {
        get => (DataTemplate?)GetValue(ItemTemplateProperty);
        set => SetValue(ItemTemplateProperty, value);
    }

    public double ItemHeight
    {
        get => (double)GetValue(ItemHeightProperty);
        set => SetValue(ItemHeightProperty, value);
    }

    public ProgressBarData? SelectedItem
    {
        get => (ProgressBarData?)GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    public ICommand? ItemClickCommand
    {
        get => (ICommand?)GetValue(ItemClickCommandProperty);
        set => SetValue(ItemClickCommandProperty, value);
    }

    /// <summary>
    ///     Path to the member/property to sort by (e.g. "Dps" or "Name").
    /// </summary>
    public string SortMemberPath
    {
        get => (string)GetValue(SortMemberPathProperty);
        set => SetValue(SortMemberPathProperty, value);
    }

    /// <summary>
    ///     Sort direction (Ascending/Descending).
    /// </summary>
    public ListSortDirection SortDirection
    {
        get => (ListSortDirection)GetValue(SortDirectionProperty);
        set => SetValue(SortDirectionProperty, value);
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        _listBox = GetTemplateChild("PART_ListBox") as ListBox;

        // If template uses ItemsSource binding in XAML (it does), the ListBox will have its ItemsSource set already.
        // Apply sorting to whatever view is currently used by the ListBox.
        ApplySorting();
    }

    private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var ctl = (SortedDpsControl)d;
        // keep legacy Data property in sync
        if (!Equals(ctl.Data, e.NewValue))
        {
            ctl.SetValue(DataProperty, e.NewValue);
        }

        ctl.ApplySorting();
    }

    private static void OnDataChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var ctl = (SortedDpsControl)d;
        // If the template binds to Data (legacy), ensure sorting is applied
        ctl.ApplySorting();
    }

    private static void OnSortChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var ctl = (SortedDpsControl)d;
        ctl.ApplySorting();
    }

    private void ApplySorting()
    {
        // Prefer the ListBox's ItemsSource (template binds to Data), fall back to Data/ItemsSource
        var source = _listBox?.ItemsSource ?? Data ?? ItemsSource;
        if (source == null)
        {
            DetachCollectionNotifications();
            return;
        }

        var view = CollectionViewSource.GetDefaultView(source);
        if (view == null)
        {
            DetachCollectionNotifications();
            return;
        }

        // If view changed, detach previous notifications
        if (!ReferenceEquals(view, _currentView))
        {
            DetachCollectionNotifications();
            _currentView = view;
            AttachCollectionNotifications(view);
        }

        using (view.DeferRefresh())
        {
            view.SortDescriptions.Clear();

            if (!string.IsNullOrWhiteSpace(SortMemberPath))
            {
                view.SortDescriptions.Add(new SortDescription(SortMemberPath, SortDirection));
            }
        }

        // Enable live sorting if available
        if (view is ICollectionViewLiveShaping { CanChangeLiveSorting: true } live)
        {
            try
            {
                live.LiveSortingProperties.Clear();
                if (!string.IsNullOrWhiteSpace(SortMemberPath))
                    live.LiveSortingProperties.Add(SortMemberPath);
                live.IsLiveSorting = true;
            }
            catch
            {
                // ignore if live shaping cannot be changed
            }

            // no need to subscribe to item PropertyChanged if live sorting is enabled
            UnsubscribeAllItems();

            // Update indices to reflect current ordering
            UpdateItemIndices(view);
            return;
        }

        // Fallback: subscribe to PropertyChanged of items and refresh view when the sort key property changes
        SubscribeToAllItems(view);

        // Update indices after applying sorting
        UpdateItemIndices(view);
    }

    private void AttachCollectionNotifications(ICollectionView view)
    {
        if (view.SourceCollection is INotifyCollectionChanged notify)
        {
            _currentCollectionNotify = notify;
            _currentCollectionNotify.CollectionChanged += OnSourceCollectionChanged;
        }
    }

    private void DetachCollectionNotifications()
    {
        if (_currentCollectionNotify != null)
        {
            _currentCollectionNotify.CollectionChanged -= OnSourceCollectionChanged;
            _currentCollectionNotify = null;
        }

        UnsubscribeAllItems();
        _currentView = null;
    }

    private void OnSourceCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        // Re-evaluate subscriptions when items are added/removed
        if (_currentView == null) return;
        SubscribeToAllItems(_currentView);

        // Update indices when collection changes
        UpdateItemIndices(_currentView);
    }

    private void SubscribeToAllItems(ICollectionView view)
    {
        // Unsubscribe first to avoid duplicates
        UnsubscribeAllItems();

        foreach (var item in view)
        {
            if (item is not INotifyPropertyChanged inpc) continue;
            PropertyChangedEventHandler handler = OnItemPropertyChanged;
            inpc.PropertyChanged += handler;
            _itemSubscriptions[inpc] = handler;
        }
    }

    private void UnsubscribeAllItems()
    {
        foreach (var kv in _itemSubscriptions)
        {
            try
            {
                kv.Key.PropertyChanged -= kv.Value;
            }
            catch
            {
                // ignore
            }
        }

        _itemSubscriptions.Clear();
    }

    private void OnItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(SortMemberPath) || e.PropertyName == null) return;

        // Compare last segment of SortMemberPath (handles nested paths)
        var lastSegment = SortMemberPath.Split('.').LastOrDefault();
        if (!string.Equals(e.PropertyName, lastSegment, StringComparison.Ordinal)) return;

        var view = _currentView ??
                   (_listBox != null ? CollectionViewSource.GetDefaultView(_listBox.ItemsSource) : null);
        if (view == null) return;

        // Refresh and update indices on UI thread, then update layout and animate
        Dispatcher.BeginInvoke(new Action(() =>
        {
            try
            {
                view.Refresh();
            }
            catch
            {
                // ignore
            }

            UpdateItemIndices(view);

            try
            {
                _listBox?.UpdateLayout();
            }
            catch
            {
                // ignore
            }

        }), System.Windows.Threading.DispatcherPriority.Loaded);
    }

    private void UpdateItemIndices(ICollectionView view)
    {
        if (view == null) return;

        int index = 1;
        foreach (var item in view)
        {
            if (item == null)
            {
                index++;
                continue;
            }

            var itemType = item.GetType();

            // Try common property names: "Id", "ID", "Order"
            var prop = itemType.GetProperty("Id") ?? itemType.GetProperty("ID") ?? itemType.GetProperty("Order");
            if (prop != null && prop.CanWrite)
            {
                try
                {
                    if (prop.PropertyType == typeof(int))
                        prop.SetValue(item, index);
                    else if (prop.PropertyType == typeof(long))
                        prop.SetValue(item, (long)index);
                    else if (prop.PropertyType == typeof(string))
                        prop.SetValue(item, index.ToString());
                }
                catch
                {
                    // ignore failures to set index
                }
            }

            index++;
        }
    }

#else
    // Backwards-compatible Data property (existing)
    public static readonly DependencyProperty DataProperty = DependencyProperty.Register(
        nameof(Data), typeof(IEnumerable), typeof(SortedDpsControl), new PropertyMetadata(null));

    // New ItemsSource property (preferred)
    public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
        nameof(ItemsSource), typeof(IEnumerable), typeof(SortedDpsControl),
        new PropertyMetadata(null, OnItemsSourceChanged));

    // ItemTemplate for customizing item visuals
    public static readonly DependencyProperty ItemTemplateProperty = DependencyProperty.Register(
        nameof(ItemTemplate), typeof(DataTemplate), typeof(SortedDpsControl), new PropertyMetadata(null));


    public static readonly DependencyProperty ItemHeightProperty = DependencyProperty.Register(
        nameof(ItemHeight), typeof(double), typeof(SortedDpsControl), new PropertyMetadata(default(double)));

    // Selected item
    public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register(
        nameof(SelectedItem), typeof(ProgressBarData), typeof(SortedDpsControl), new PropertyMetadata(null));

    // Item click command
    public static readonly DependencyProperty ItemClickCommandProperty = DependencyProperty.Register(
        nameof(ItemClickCommand), typeof(ICommand), typeof(SortedDpsControl), new PropertyMetadata(null));

    static SortedDpsControl()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(SortedDpsControl),
            new FrameworkPropertyMetadata(typeof(SortedDpsControl)));
    }

    public IEnumerable? Data
    {
        get => (IEnumerable?)GetValue(DataProperty);
        set => SetValue(DataProperty, value);
    }

    public IEnumerable? ItemsSource
    {
        get => (IEnumerable?)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public DataTemplate? ItemTemplate
    {
        get => (DataTemplate?)GetValue(ItemTemplateProperty);
        set => SetValue(ItemTemplateProperty, value);
    }

    public double ItemHeight
    {
        get => (double)GetValue(ItemHeightProperty);
        set => SetValue(ItemHeightProperty, value);
    }

    public ProgressBarData? SelectedItem
    {
        get => (ProgressBarData?)GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    public ICommand? ItemClickCommand
    {
        get => (ICommand?)GetValue(ItemClickCommandProperty);
        set => SetValue(ItemClickCommandProperty, value);
    }

    private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var ctl = (SortedDpsControl)d;
        // keep legacy Data property in sync
        if (!Equals(ctl.Data, e.NewValue))
        {
            ctl.SetValue(DataProperty, e.NewValue);
        }
    }
#endif 
}