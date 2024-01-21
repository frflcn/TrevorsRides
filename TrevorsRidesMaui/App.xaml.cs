#if (ANDROID || IOS)

using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Timers;
using MauiLocation = Microsoft.Maui.Devices.Sensors;

using TrevorsRidesHelpers;
using System.Windows.Input;
using Microsoft.Maui.Storage;
using TrevorsRidesMaui.BackgroundTasks;
using TrevorsRidesHelpers.Ride;
using Maui.GoogleMaps;


namespace TrevorsRidesMaui
{
    public partial class App : Application
    {
        public static bool IsLoggedIn { get; set; } = false;
        public static bool RideRequested { get; set; }
        public static HttpClient HttpClient { get; set; }
        public static AccountSession? AccountSession { get; set; }
        public static bool IsTesting { get; set; } = false;
        public static JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            Converters =
                {
                    new Json.PhoneNumberJsonConverter()
                }
        };

        public static DriverStatus? TrevorsStatus;
        //public static Pin TrevorsLocation = new Pin()
        //{
        //    Label = "Trevor is here",
        //    Type = PinType.Place
        //};






        static App()
        {
            RideRequested = false;
            HttpClient = new HttpClient();
            HttpClient.Timeout = TimeSpan.FromSeconds(20);
        }





        public App()
        {
            InitializeComponent();

            MainPage = new NavigationPage(new LoginPage());

            //MainPage = new NavigationPage(new MyNavigationFlyoutPage());
            //MainPage= new MyFlyoutPage();




        }





        protected override void OnStart()
        {
            
            base.OnStart();
        }





        protected override Window CreateWindow(IActivationState? activationState)
        {
            
            Window window = base.CreateWindow(activationState);

            window.Created += async (s, e) =>
            {
                Log.Debug("CREATED WINDOW");

                string isTestingString = await SecureStorage.GetAsync("IsTesting");
                if (!string.IsNullOrEmpty(isTestingString))
                {
                    IsTesting = bool.Parse(isTestingString);
                }


                await CheckPermissions();


                if (App.AccountSession != null)
                {
                    if (!RideRequestService.IsRunning)
                    {
                        if (App.IsLoggedIn)
                        {
                            RideRequestService.StartService();
                        }
                    }
                }



            };
            window.Resumed += (s, e) =>
            {
                if (App.AccountSession != null)
                {
                    if (!RideRequestService.IsRunning)
                    {
                        if (App.IsLoggedIn)
                        {
                            RideRequestService.StartService();
                        }
                    }
                }
            };

            window.Deactivated += (s, e) =>
            {
                Log.Debug("Deactivated");
            };
            window.Stopped += (s, e) =>
            {
                Log.Debug("Stopped");
                if (RideRequested == false)
                {
                    RideRequestService.StopService();
                }
            };
            window.Destroying += (s, e) =>
            {

                
                
                Log.Debug("Destroying");
            };
            return window;
        }





        public async void Setup()
        {

        }





  





        public static async Task<bool> CheckPermissions()
        {
            PermissionStatus locationStatus = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
            if (locationStatus != PermissionStatus.Granted)
            {
                locationStatus = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
                if (locationStatus == PermissionStatus.Granted)
                {
                    return true;
                }
                else
                    return false;
            }
            else
                return true;
        }
    }
}
#endif