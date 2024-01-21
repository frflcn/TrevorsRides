using TrevorsRidesHelpers;

namespace TrevorsRidesMaui;

public partial class MyFlyoutPage : FlyoutPage
{
    NavigationPage mainPage = new NavigationPage(new MainPage());
	public MyFlyoutPage()
	{
		InitializeComponent();
        Detail = mainPage;
        flyoutPage.collectionView.SelectionChanged += OnSelectionChanged;
    }
    protected override void OnAppearing()
    {
        base.OnAppearing();
        foreach(var thing in ToolbarItems)
        {
            Log.Debug("ToolBarItems");
            Log.Debug(thing.IconImageSource.ToString());
            Log.Debug(thing.Text);
        }
    }
    void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var item = e.CurrentSelection.FirstOrDefault() as FlyoutPageItem;
        if (item != null)
        {
            if (item.TargetType == typeof(MainPage))
            {
                Detail = mainPage;
                IsPresented = false;
                return;
            }
            Detail = new NavigationPage((Page)Activator.CreateInstance(item.TargetType));
            IsPresented = false;
        }
        
    }
}