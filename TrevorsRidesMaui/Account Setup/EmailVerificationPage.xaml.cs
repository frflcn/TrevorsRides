using TrevorsRidesHelpers;
namespace TrevorsRidesMaui.Account_Setup;

public partial class EmailVerificationPage : ContentPage
{
    private static AccountSetup _account;
    public static AccountSetup Account
    {
        get { return _account; }
        set { _account = value; }
    }
    private HttpClient _httpClient;
	public EmailVerificationPage(ref AccountSetup accountSetup)
	{
		Account = accountSetup;
		InitializeComponent();
		CodeLabel.Text = $"Please enter the verification code sent to {Account.Email}";
		_httpClient = App.HttpClient;
		
	}

    private async void CodeEntry_TextChanged(object sender, TextChangedEventArgs e)
    {
		Uri uri = new Uri($"{Helpers.Domain}/api/Setup/VerifyEmail");
		if (e.NewTextValue.Length == 6)
		{
			int length;
			if (!int.TryParse(e.NewTextValue, out length))
			{
				return;
			}
			HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, uri);
			request.Headers.Add("Email", Account.Email);
			request.Headers.Add("VerificationCode", e.NewTextValue);
			request.Headers.Add("Identifier", Account.Identifier.ToString());
			HttpResponseMessage response = await _httpClient.SendAsync(request);
			string body = await response.Content.ReadAsStringAsync();
			if (body == "Success")
			{
				await Navigation.PushAsync(new PhonePage(ref _account));
			}
			else if (body == "Verification Code has expired, please request a new one")
			{
				if (await DisplayAlert("Verification code has Expired", body, "Resend", "Cancel"))
				{
					EmailPage.SendEmail();
                }
			} 
			else if (body == "Invalid Verification Code")
			{

				int distance = 10;
				uint timeInterval = 100;
				CodeEntry.TextColor = Color.FromRgb(255, 0, 0);
                await CodeEntry.TranslateTo(-distance / 2, 0, timeInterval);
				for (int i = 0; i < 4; i++)
				{
					await CodeEntry.TranslateTo(distance, 0, timeInterval);
					await CodeEntry.TranslateTo(-distance, 0, timeInterval);
				}
				await CodeEntry.TranslateTo(0, 0, timeInterval);
                CodeEntry.TextColor = Color.FromRgb(0, 0, 0);

            }
		}
    }
}