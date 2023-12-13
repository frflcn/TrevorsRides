using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using TrevorsRidesHelpers;



namespace TrevorsRidesMaui.Account_Setup;

public partial class PasswordPage : ContentPage
{
	public AccountSetup Account {  get; set; }

	public PasswordPage(ref AccountSetup account)
	{
		Account = account;
		InitializeComponent();

	}
	public async void SubmitButton_Pressed(object sender, EventArgs e)
	{
		if (PasswordEntry.Text == ConfirmPasswordEntry.Text)
		{
			HttpClient client = App.HttpClient;
			ByteArrayContent content = new ByteArrayContent(new byte[0]);
			content.Headers.Add("Password", PasswordEntry.Text);
			content.Headers.Add("Identifier", Account.Identifier.ToString());
			content.Headers.Add("NationalPhoneNumber", Account.Phone.NationalNumber.ToString());
			content.Headers.Add("CountryCode", Account.Phone.CountryCode.ToString());
			HttpResponseMessage response = await client.PostAsync($"{Helpers.Domain}/api/CreateAccount", content);
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
			await SecureStorage.Default.SetAsync("AccountSession", await response.Content.ReadAsStringAsync());
            App.AccountSession = await response.Content.ReadFromJsonAsync<AccountSession>(jsonOptions);
			Application.Current.MainPage = new NavigationPage(new MainPage());

        }
		else
		{
			await DisplayAlert("Password", "Passwords don't match", "Ok");
		}
		
	}
}