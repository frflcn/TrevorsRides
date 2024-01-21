using PhoneNumbers;
using TrevorDrivesMaui.BackgroundTasks;


namespace TrevorDrivesMaui;

public partial class AccountPage : ContentPage
{
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
		//TODO: Check to make sure Driver is not in the middle of a trip
		RideRequestService.StopService();
		App.IsLoggedIn = false;
		await SecureStorage.SetAsync("AccountSession", "");
        App.AccountSession = null;
        App.Current.MainPage = new NavigationPage(new LoginPage());
		
		
    }
	protected override void OnAppearing()
	{
		Title = "";
	}
}