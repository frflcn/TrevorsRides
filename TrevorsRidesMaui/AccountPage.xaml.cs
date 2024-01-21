using PhoneNumbers;
using TrevorsRidesMaui.BackgroundTasks;

namespace TrevorsRidesMaui;

public partial class AccountPage : ContentPage
{
	string lastTapped = "phone";
	int count = 0;
	public AccountPage()
	{
		InitializeComponent();

		PhoneNumberUtil util = PhoneNumberUtil.GetInstance();
		Name.Text = App.AccountSession.Account.FirstName + " " + App.AccountSession.Account.LastName;
		Email.Text = App.AccountSession.Account.Email;	
		PhoneNumber.Text = util.Format(App.AccountSession.Account.PhoneNumber, PhoneNumberFormat.NATIONAL);

    }

    private async void Logout_Clicked(object sender, EventArgs e)
    {
		RideRequestService.StopService();
		App.IsLoggedIn = false;
		await SecureStorage.SetAsync("AccountSession", "");
        App.AccountSession = null;
        App.Current.MainPage = new NavigationPage(new LoginPage());
		
		
    }
	protected override async void OnAppearing()
	{
		Title = "";
		string HasTestBeenDiscovered = await SecureStorage.GetAsync("HasTestBeenDiscovered");
		if (!string.IsNullOrEmpty(HasTestBeenDiscovered) && bool.Parse(HasTestBeenDiscovered))
		{
			TestingLayout.IsVisible = true;
		}
		TestingSwitch.IsToggled = App.IsTesting;
        //bool HasTestBeenDiscovered = bool.Parse(!)!;
	}

    private async void TestingSwitch_Toggled(object sender, ToggledEventArgs e)
    {
		App.IsTesting = e.Value;
		await SecureStorage.SetAsync("IsTesting", e.Value.ToString());
    }

    private void LabelName_Tapped(object sender, TappedEventArgs e)
    {
		if(lastTapped == "phone")
		{
			lastTapped = "name";
		}
		else
		{
			count = 0;
			lastTapped = "phone";
		}
    }
    private void LabelEmail_Tapped(object sender, TappedEventArgs e)
    {
        if (lastTapped == "name")
        {
            lastTapped = "email";
        }
        else
        {
            count = 0;
            lastTapped = "phone";
        }
    }
    private async void LabelPhone_Tapped(object sender, TappedEventArgs e)
    {
        if (lastTapped == "email")
        {
            count++;
            if (count == 3)
            {
                string hasTestingBeenDiscovered = await SecureStorage.GetAsync("HasTestBeenDiscovered") ?? bool.FalseString;
                if (bool.Parse(hasTestingBeenDiscovered))
                {
                    await SecureStorage.SetAsync("HasTestBeenDiscovered", bool.FalseString);
                    TestingSwitch.IsEnabled = false;
                    TestingSwitch.IsToggled = false;
                    await Task.Delay(1500);
                    TestingLayout.IsVisible = false;
                }
                else
                {
                    await SecureStorage.SetAsync("HasTestBeenDiscovered", bool.TrueString);
                    TestingLayout.IsVisible = true;
                    TestingSwitch.IsEnabled = true;
                }
                count = 0;
            }
            lastTapped = "phone";
            
        }
        else
        {
            count = 0;
            lastTapped = "phone";
        }
    }
}