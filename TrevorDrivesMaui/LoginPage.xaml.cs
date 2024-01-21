using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using TrevorsRidesHelpers;
using TrevorDrivesMaui.Account_Setup;
using TRH = TrevorsRidesHelpers;
using PhoneNumbers;
using TrevorDrivesMaui.BackgroundTasks;

namespace TrevorDrivesMaui;

public partial class LoginPage : ContentPage
{
	Random random = new Random();
	HttpClient httpClient;

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
		httpClient = App.HttpClient;
		InitializeComponent();
		IsSupported = true;
		ContactedServer = false;
    }





    protected async override void OnAppearing()
    {
        base.OnAppearing();
        Task checkVersionTask = CheckVersion();
        Task autoLoginTask = AutoLogin();
        await Task.Delay(1000);
        Log.Debug("AUTO LOGIN", $"Status: {autoLoginTask.Status}");
        Log.Debug("CHECK VERSION", $"Status: {checkVersionTask.Status}");

        await Task.WhenAll(checkVersionTask, autoLoginTask);
        ContactedServer = true;
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
		HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"{Helpers.Domain}/api/Driver/Login");
		request.Headers.Add("Email", EmailEntry.Text);
		request.Headers.Add("Password", PasswordEntry.Text);
		HttpResponseMessage response = await httpClient.SendAsync(request);

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
            try
            {
                await SecureStorage.Default.SetAsync("AccountSession", await response.Content.ReadAsStringAsync());
            }
            catch (Exception ex)
            {
                Console.WriteLine("This is an output write to secure storage failed");
            }

			App.AccountSession = await response.Content.ReadFromJsonAsync<AccountSession>(jsonOptions);
			App.IsLoggedIn = true;
			//Application.Current.MainPage = new NavigationPage(new MainPage());
			App.Current.MainPage = new MyFlyoutPage();
			RideRequestService.StartService();
			
        }
		else
		{
            int distance = 10;
            uint timeInterval = 100;
            IncorrectPasswordLabel.IsVisible = true;
            await IncorrectPasswordLabel.TranslateTo(-distance / 2, 0, timeInterval);
            for (int i = 0; i < 4; i++)
            {
                await IncorrectPasswordLabel.TranslateTo(distance, 0, timeInterval);
                await IncorrectPasswordLabel.TranslateTo(-distance, 0, timeInterval);
            }
            await IncorrectPasswordLabel.TranslateTo(0, 0, timeInterval); IncorrectPasswordLabel.IsVisible = true;
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
        (Application.Current.MainPage as NavigationPage).PushAsync(new NamePage());
    }






	private async Task<bool> AutoLogin()
	{
        string accountSessionJson = await SecureStorage.Default.GetAsync("AccountSession");

        if (string.IsNullOrEmpty(accountSessionJson))
        {
			return false;
        }

        App.AccountSession = JsonSerializer.Deserialize<AccountSession>(accountSessionJson, Json.Options);
        Uri uri = new Uri($"{Helpers.Domain}/api/Driver/Login");
		HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, uri);
		request.Headers.Add("User-ID", App.AccountSession.Account.Id.ToString());
		request.Headers.Add("SessionToken", App.AccountSession.SessionToken.Token);
        HttpResponseMessage response;
        try
		{
			Log.Debug("AUTO LOGIN", "Before Request");
			Task<HttpResponseMessage> responseTask = httpClient.SendAsync(request);
			await Task.Delay(1000);
			Log.Debug("AUTO LOGIN", $"Actual request: {responseTask.Status}");
			response = await responseTask;
            Log.Debug("AUTO LOGIN", "After Request");
        }
		catch (Exception ex)
		{
			Log.Debug(ex.Message);
			Log.Debug(ex.StackTrace ?? "Error Auto Logging In");
			return false;
		}
		if (response.StatusCode == HttpStatusCode.OK)
		{
			
			Log.Debug("AUTO LOGIN", "Status OK");

			accountSessionJson = await response.Content.ReadAsStringAsync();
			Log.Debug("AUTO LOGIN", $"Json: {accountSessionJson}");
            await SecureStorage.Default.SetAsync("AccountSession", accountSessionJson);
			App.AccountSession = JsonSerializer.Deserialize<AccountSession>(accountSessionJson, Json.Options);
			App.IsLoggedIn = true;
			//App.Current.MainPage = new NavigationPage(new MainPage());
			App.Current.MainPage = new MyFlyoutPage();
            RideRequestService.StartService();

			return true;
        }
        Log.Debug("AUTO LOGIN", "Status NOT OK");
        Log.Debug("AUTO LOGIN", await response.Content.ReadAsStringAsync());
        return false;
	}





	private async Task CheckVersion()
	{
		await _CheckVersion(0);
	}





	private async Task _CheckVersion(int retryCount)
	{

		Log.Debug("CheckVErsion");
		HttpResponseMessage response;
		Uri uri = new Uri($"{Helpers.Domain}/api/Version/Driver");
		Log.Debug(uri.OriginalString);
		Log.Debug(uri.AbsolutePath);
		Log.Debug(uri.AbsoluteUri);
		Log.Debug(JsonSerializer.Serialize<HttpClient>(httpClient));
        try
		{
			Task<HttpResponseMessage> responseTask = httpClient.GetAsync(uri);
			//response = await httpClient.GetAsync(uri);
			await Task.Delay(1000);
			Log.Debug("CHECK VERSION", $"Actual Status: {responseTask.Status}");
			response = await responseTask;
        }
		catch (Exception ex)
		{
			Log.Debug("ERRROR", ex.Message);
			return;
		}

		HttpContent content = response.Content;
		HttpStatusCode code = response.StatusCode;
		if (code != HttpStatusCode.OK)
		{
            await Task.Delay(1000);
            if (retryCount < 10)
			{
                await _CheckVersion(++retryCount);
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