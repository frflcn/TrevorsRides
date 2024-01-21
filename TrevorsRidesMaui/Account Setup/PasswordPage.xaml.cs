using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using TrevorsRidesHelpers;
using TrevorsRidesMaui.BackgroundTasks;



namespace TrevorsRidesMaui.Account_Setup;

public partial class PasswordPage : ContentPage
{
	public AccountSetup Account {  get; set; }
	private Color GoodColor = Colors.Green;
	private Color BadColor = Colors.Red;
	public static readonly char[] SpecialCharacters = ['`', '~', '!', '@', '#', '$', '%', '^', '&', '*', '(', ')', '_', '-', '+', '=', '{', '}', '[', ']', '|', '\\', ':', ';', '"', '\'', '<', '>', ',', '.', '?', '/'];
	public PasswordPage(ref AccountSetup account)
	{
		Account = account;
		InitializeComponent();


	}
	public async void SubmitButton_Pressed(object sender, EventArgs e)
	{
		if (CheckPasswords())
		{
			HttpClient client = App.HttpClient;
			ByteArrayContent content = new ByteArrayContent(new byte[0]);
			content.Headers.Add("Password", PasswordEntry.Text);
			content.Headers.Add("User-ID", Account.Identifier.ToString());
			content.Headers.Add("NationalPhoneNumber", Account.Phone.NationalNumber.ToString());
			content.Headers.Add("CountryCode", Account.Phone.CountryCode.ToString());
			HttpResponseMessage response = await client.PostAsync($"{Helpers.Domain}/api/Rider/CreateAccount", content);
			if (response.StatusCode == HttpStatusCode.BadRequest)
			{
				string body = await response.Content.ReadAsStringAsync();
				await DisplayAlert("Error", body, "Ok");
				return;
			}
			if (response.StatusCode != HttpStatusCode.OK)
			{
                await DisplayAlert("Error", "Unknown Error", "Ok");
                return;
            }
			Log.Debug("Password Page: ", await response.Content.ReadAsStringAsync());
            JsonSerializerOptions jsonOptions = new JsonSerializerOptions
            {
                Converters =
                {
                    new Json.PhoneNumberJsonConverter()
                }
            };
			try
			{
                await SecureStorage.Default.SetAsync("AccountSession", await response.Content.ReadAsStringAsync());
            }
			catch (Exception ex)
			{
				Console.WriteLine("This is an output write to secure storage failed");
			}
			RideRequestService.StartService();
			App.IsLoggedIn = true;
            App.AccountSession = await response.Content.ReadFromJsonAsync<AccountSession>(jsonOptions);
            //Application.Current.MainPage = new NavigationPage(new MainPage());
            Application.Current.MainPage = new MyFlyoutPage();
        }
		else
		{
            int distance = 10;
            uint timeInterval = 100;
            InstructionsLayout.IsVisible = true;
            await InstructionsLayout.TranslateTo(-distance / 2, 0, timeInterval);
            for (int i = 0; i < 4; i++)
            {
                await InstructionsLayout.TranslateTo(distance, 0, timeInterval);
                await InstructionsLayout.TranslateTo(-distance, 0, timeInterval);
            }
            await InstructionsLayout.TranslateTo(0, 0, timeInterval);
            //await DisplayAlert("Password", "Passwords don't match", "Ok");
		}
		
	}
	private bool CheckPasswords()
	{
		string password = PasswordEntry.Text;
		if (ConfirmPasswordEntry.Text != PasswordEntry.Text)
		{
			return false;
		}
		if (!password.Any(e => char.IsDigit(e)))
		{
			return false;
		}
        if (!password.Any(e => char.IsLower(e)))
        {
            return false;
        }
        if (!password.Any(e => char.IsUpper(e)))
        {
            return false;
        }
        if (!password.Any(e => char.IsSymbol(e) ||  char.IsPunctuation(e)))
        {
            return false;
        }
		if (password.Length < 8)
		{
			return false;
		}
		return true;
    }

    private void PasswordEntry_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (e.NewTextValue == ConfirmPasswordEntry.Text)
        {
            MatchLabel.TextColor = GoodColor;
        }
        else
        {
            MatchLabel.TextColor = BadColor;
        }
		if (e.NewTextValue.Any(e => char.IsDigit(e)))
		{
			DigitLabel.TextColor = GoodColor;
		}
		else
		{
			DigitLabel.TextColor= BadColor;
		}
		if (e.NewTextValue.Any(e => char.IsLower(e)))
		{
			LowercaseLabel.TextColor = GoodColor;
		}
		else
		{
			LowercaseLabel.TextColor= BadColor;
		}
		if (e.NewTextValue.Any(e => char.IsUpper(e)))
		{
			UppercaseLabel.TextColor = GoodColor;
		}
		else
		{
			UppercaseLabel.TextColor= BadColor;
		}
		if (e.NewTextValue.Any(e => char.IsSymbol(e) || char.IsPunctuation(e)))
		{
			SpecialCharLabel.TextColor = GoodColor;
		}
		else
		{
			SpecialCharLabel.TextColor = BadColor;
		}
		if (e.NewTextValue.Length >= 8)
		{
			LengthLabel.TextColor = GoodColor;
		}
		else
		{
			LengthLabel.TextColor = BadColor;
		}
    }

    private void ConfirmPasswordEntry_TextChanged(object sender, TextChangedEventArgs e)
    {
		if (e.NewTextValue == PasswordEntry.Text)
		{
			MatchLabel.TextColor = GoodColor;
		}
		else
		{
			MatchLabel.TextColor = BadColor;
		}
    }
}