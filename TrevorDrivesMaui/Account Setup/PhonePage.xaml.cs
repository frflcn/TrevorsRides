using PhoneNumbers;
using TrevorsRidesHelpers;

namespace TrevorDrivesMaui.Account_Setup;

public partial class PhonePage : ContentPage
{
    PhoneNumberUtil util = PhoneNumberUtil.GetInstance();
    private AccountSetup _account;
    public AccountSetup Account
    {
        get { return _account; }
        set { _account = value; }
    }
	public PhonePage(ref AccountSetup account)
	{
		Account = account;
		InitializeComponent();
		
	}

    private void PhoneEntry_TextChanged(object sender, TextChangedEventArgs e)
    {

		AsYouTypeFormatter formatter = util.GetAsYouTypeFormatter("US");
		
    }

    private async void Button_Clicked(object sender, EventArgs e)
    {
		try
		{
            Account.Phone = util.Parse(PhoneEntry.Text, "US");
            _ = Navigation.PushAsync(new PasswordPage(ref _account));
            
        }
		catch (NumberParseException)
        {
            int distance = 10;
            uint timeInterval = 100;
            PhoneEntry.TextColor = Color.FromRgb(255, 0, 0);
            await PhoneEntry.TranslateTo(-distance / 2, 0, timeInterval);
            for (int i = 0; i < 4; i++)
            {
                await PhoneEntry.TranslateTo(distance, 0, timeInterval);
                await PhoneEntry.TranslateTo(-distance, 0, timeInterval);
            }
            await PhoneEntry.TranslateTo(0, 0, timeInterval);
            PhoneEntry.TextColor = Color.FromRgb(0, 0, 0);

        }
		
    }
}