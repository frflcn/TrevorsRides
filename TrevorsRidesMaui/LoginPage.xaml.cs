using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using TrevorsRidesHelpers;
using TrevorsRidesMaui.Account_Setup;
using TRH = TrevorsRidesHelpers;
using PhoneNumbers;

namespace TrevorsRidesMaui;

public partial class LoginPage : ContentPage
{
	private HttpClient _httpClient;
	public bool IsSupported { get; set; }
	public bool ContactedServer { get; set; }
	public LoginPage()
	{
		PhoneNumberUtil util = PhoneNumberUtil.GetInstance();
		PhoneNumber number = util.Parse("6104136280", "US");
		JsonSerializerOptions jsonOptions  = new JsonSerializerOptions
        {
            Converters =
				{
					new Json.PhoneNumberJsonConverter()
				}
        };
        Log.Debug("Json Serializer PHone: ", JsonSerializer.Serialize<PhoneNumber>(number, jsonOptions));
		_httpClient = App.HttpClient;
		InitializeComponent();
		IsSupported = true;
		ContactedServer = false;



    }
	public async void LoginButton_Pressed(object sender, EventArgs e)
    {
		if (!IsSupported)
		{
			DisplayAlert("Update App", "This app version is no longer supported. Please update now at https://www.trevorsrides.com/Download", "Ok");
			return;
        }
		if (!ContactedServer)
		{
			DisplayAlert("Please Wait", "Please wait while we attempt to reach the server", "Ok");
		}
		HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"{Helpers.Domain}/api/Login");
		request.Headers.Add("Email", EmailEntry.Text);
		request.Headers.Add("Password", PasswordEntry.Text);
		HttpResponseMessage response = await _httpClient.SendAsync(request);

        JsonSerializerOptions jsonOptions = new JsonSerializerOptions
        {
            Converters =
                {
                    new Json.PhoneNumberJsonConverter()
                }
        };
		Log.Debug("LOGIN", await response.Content.ReadAsStringAsync());
        if (response.StatusCode == HttpStatusCode.OK)
		{
			await SecureStorage.Default.SetAsync("AccountSession", await response.Content.ReadAsStringAsync());
			App.AccountSession = await response.Content.ReadFromJsonAsync<AccountSession>(jsonOptions);
            Application.Current.MainPage = new NavigationPage(new MainPage());
			
        }
		else
		{
			IncorrectPasswordLabel.IsVisible = true;
		}
    }
		
    

    private void CreateAccountButton_Pressed(object sender, EventArgs e)
    {
        if (!IsSupported)
        {
            DisplayAlert("Update App", "This app version is no longer supported. Please update now at https://www.trevorsrides.com/Download", "Ok");
            return;
        }
        if (!ContactedServer)
        {
            DisplayAlert("Please Wait", "Please wait while we attempt to reach the server", "Ok");
			return;
        }
        //Application.Current.MainPage = new NavigationPage(new CreateAccountPage());
        (Application.Current.MainPage as NavigationPage).PushAsync(new CreateAccountPage());
    }
	protected async override void OnAppearing()
	{
		base.OnAppearing();
        Log.Debug("OnApperaing", "PRior");
        await CheckVersion();
		Log.Debug("OnApperaing");
    }
	private async Task CheckVersion()
	{
		await _CheckVersion(0);
	}
	private async Task _CheckVersion(int retryCount)
	{

		Log.Debug("CheckVErsion");
		HttpResponseMessage response;
		Uri uri = new Uri($"{Helpers.Domain}/api/Version/Rider");
		Log.Debug(uri.OriginalString);
		Log.Debug(uri.AbsolutePath);
		Log.Debug(uri.AbsoluteUri);
		Log.Debug(JsonSerializer.Serialize<HttpClient>(_httpClient));
        try
		{
			_httpClient = new HttpClient();
            //response= await _httpClient.GetAsync(uri).ConfigureAwait(false);
			response = await _httpClient.GetAsync(uri).ConfigureAwait(false);
			

			Thread.Sleep(1000);
			//Log.Debug(task.IsCompleted.ToString());
			//Log.Debug(task.IsFaulted.ToString());
			////Log.Debug("AsyncState", task.AsyncState);
			//Log.Debug("Status", task.Status.ToString());

			//response = await task;
        }
		catch (Exception ex)
		{
			Log.Debug("ERRROR", ex.Message);
			return;
		}
		
		Log.Debug("Received Response");
		HttpContent content = response.Content;
		HttpStatusCode code = response.StatusCode;
		if (code != HttpStatusCode.OK)
		{
			if (retryCount < 10)
			{
                _CheckVersion(++retryCount);
				return;
            }
			_ = DisplayAlert("Server Unavailable", "The server is unavailable at this time", "Ok");
			return;
		}
		
		VersionControl versionControl = JsonSerializer.Deserialize<VersionControl>(await content.ReadAsStringAsync())!;
		Version thisVersion = AppInfo.Version;
		if (thisVersion < versionControl.MinimumVersion)
		{
			IsSupported = false;
			_ = DisplayAlert("Update App", "This app version is no longer supported. Please update now at https://www.trevorsrides.com/Download", "Ok");
			return;
		}
        ContactedServer = true;
        IsSupported = true;
		if (thisVersion.Major < versionControl.LatestVersion.Major)
		{
			if (thisVersion.Major == 0)
			{
				_ = DisplayAlert("Update App", "There is a newer, fully working version of this app. Update now at https://www.trevorsrides.com/Download", "Ok");
				return;
			}
			_ = DisplayAlert("Update App", "There's a newer version of this app. Update now at https://www.trevorsrides.com/Download.", "Ok");
			return;
		}
        if (thisVersion.Minor < versionControl.LatestVersion.Minor)
        {
            _ = DisplayAlert("Update App", "There's a newer version of this app. Update now at https://www.trevorsrides.com/Download.", "Ok");
			return;
        }
    }
}