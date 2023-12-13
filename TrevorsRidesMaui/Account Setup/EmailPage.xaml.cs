using System.Net;
using System.Net.Http.Json;
using System.Net.Mail;
using TrevorsRidesHelpers;

namespace TrevorsRidesMaui.Account_Setup;

public partial class EmailPage : ContentPage
{
	private static AccountSetup _account;
	public static AccountSetup Account { 
		get { return _account; } 
		set { _account = value; } 
	}
	public EmailPage(ref AccountSetup account)
	{
		Account = account;

		InitializeComponent();
	}

    private void NextButton_Pressed(object sender, EventArgs e)
    {
		Account.Email = this.EmailEntry.Text;
		SendEmail();

		Navigation.PushAsync(new EmailVerificationPage(ref _account));
    }
	public static async void SendEmail()
	{
        HttpClient client = App.HttpClient;
        Uri uri = new Uri($"{Helpers.Domain}/api/Setup/VerifyEmail");
		ByteArrayContent content = new ByteArrayContent(new byte[0]);
		content.Headers.Add("Email", Account.Email);
		content.Headers.Add("FirstName", Account.FirstName);
		content.Headers.Add("LastName", Account.LastName);
		content.Headers.Add("Guid", Account.Identifier.ToString());
		HttpResponseMessage response = await client.PostAsync(uri, content);
		Log.Debug(response.StatusCode.ToString());
		Log.Debug(await response.Content.ReadAsStringAsync());
		//if (response.)
		
	}


}