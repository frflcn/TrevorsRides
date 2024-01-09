using TrevorsRidesHelpers;

namespace TrevorsRidesMaui;

public partial class BookRidePage : ContentPage
{
	public BookRidePage(string url)
	{
		InitializeComponent();
		CheckoutPage.Source = url;
	}

    private void CheckoutPage_Navigating(object sender, WebNavigatingEventArgs e)
    {
		if (e.Url == "https://www.example.com/success/")
		{
			e.Cancel = true;
			App.Current.MainPage = new RideInProgressPage();
		}
    }
}