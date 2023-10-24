using System.Collections.ObjectModel;
using System.Windows.Input;
using Android.Util;
using Microsoft.Maui.Controls;

namespace TrevorsRidesMaui.Controls;

public partial class ComboBox : ContentView
{
   
    public BindableProperty PlaceholderProperty = BindableProperty.Create(nameof(Placeholder), typeof(string), typeof(ComboBox), propertyChanged: (bindable, oldValue, newValue) =>
        {
            var control = (ComboBox)bindable;
            control.TextEditor.Placeholder = newValue as string;
        });
    public string Placeholder
    {
        get => GetValue(PlaceholderProperty) as string;
        set => SetValue(PlaceholderProperty, value);
    }


    public static readonly BindableProperty ItemsSourceProperty = BindableProperty.Create(nameof(ItemsSource), typeof(ObservableCollection<ListViewEntry>), typeof(ComboBox), propertyChanged: (bindable, oldValue, newValue) =>
        {
            var control = (ComboBox)bindable;
            control.Suggestions.ItemsSource = newValue as ObservableCollection<ListViewEntry>;
        });
    public ObservableCollection<ListViewEntry> ItemsSource
    {
        get => GetValue(ItemsSourceProperty) as ObservableCollection<ListViewEntry>;
        set => SetValue(ItemsSourceProperty, value);
    }


    public event EventHandler<TextChangedEventArgs> OnTextChanged;
    public event EventHandler<ItemTappedEventArgs> OnItemTapped;

    

    public ComboBox()
	{
        InitializeComponent();
    }
  

    private void OnFocused(object sender, FocusEventArgs e)
    {
        this.Suggestions.IsVisible= true;
    }
    private void OnUnfocused(object sender, FocusEventArgs e)
    {
        this.Suggestions.IsVisible = false;
    }
    private void EntryTextChanged(object sender, TextChangedEventArgs e)
    {
        OnTextChanged?.Invoke(this, e);
    }
    private void OnListViewItemTapped(object sender, ItemTappedEventArgs e)
    {
        OnItemTapped?.Invoke(this, e);
    }
   
}