namespace TrevorsRidesMaui;

public partial class RideInProgressPage : ContentPage
{
	public RideInProgressPage()
	{
		InitializeComponent();
	}

    private void Button_Clicked(object sender, EventArgs e)
    {
		MainThread.BeginInvokeOnMainThread(() =>
		{
			App.Current.MainPage = new MyFlyoutPage();
        });
    }
}