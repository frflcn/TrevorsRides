using System.Windows.Input;
using TrevorsRidesHelpers;

namespace TrevorDrivesMaui.Account_Setup;

public partial class NamePage : ContentPage
{


    public NamePage()
	{
		InitializeComponent();
        BindingContext = this;

	}

    private void NextButton_Pressed(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(FirstNameEntry.Text) || string.IsNullOrEmpty(LastNameEntry.Text))
        {
            _ = DisplayAlert("", "Please Fill in your Name", "Ok");
            return;
        }
        AccountSetup account = new AccountSetup(
			FirstNameEntry.Text,
			LastNameEntry.Text );
		Navigation.PushAsync(new EmailPage(ref account));
    }


    private void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {
        (App.Current.MainPage as NavigationPage).PushAsync(new PrivacyPolicy());
    }
}