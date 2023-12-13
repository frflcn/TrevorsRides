using System.Windows.Input;
using TrevorsRidesHelpers;

namespace TrevorsRidesMaui.Account_Setup;

public partial class CreateAccountPage : ContentPage
{


    public CreateAccountPage()
	{
		InitializeComponent();
        BindingContext = this;

	}

    private void NextButton_Pressed(object sender, EventArgs e)
    {
		AccountSetup account = new AccountSetup(
			FirstNameEntry.Text,
			LastNameEntry.Text );
		Navigation.PushAsync(new EmailPage(ref account));
    }



    private void WebView_Navigating(object sender, WebNavigatingEventArgs e)
    {
        Log.Debug("Webview a");
        Log.Debug(e.Url);
        if (e.Url == "https://www.notawebsite.com/")
        {
            e.Cancel = true;
            (Application.Current.MainPage as NavigationPage).PushAsync(new PrivacyPolicy());
        }
    }
}