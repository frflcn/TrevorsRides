#if (ANDROID || IOS)

using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Timers;
using MauiLocation = Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Controls.Maps;
using TrevorsRidesHelpers;
using System.Windows.Input;
using Microsoft.Maui.Storage;
using TrevorsRidesMaui.BackgroundTasks;
using TrevorsRidesHelpers.Ride;


namespace TrevorsRidesMaui
{
    public partial class App : Application
    {
        public static bool RideRequested { get; set; }
        public static HttpClient HttpClient { get; set; }
        public ClientWebSocket client;
        public static AccountSession? AccountSession { get; set; }
        public static JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            Converters =
                {
                    new Json.PhoneNumberJsonConverter()
                }
        };

        public static DriverStatus? TrevorsStatus;
        public static Pin TrevorsLocation = new Pin()
        {
            Label = "Trevor is here",
            Type = PinType.Place
        };

        public string Username { get; set; }
        public string Password { get; set; }





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
            //Username = await SecureStorage.GetAsync(Username);
            //MainPage = new AppShell();
            //MainPage = new NavigationPage(new LoginPage());




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


                



                await CheckPermissions();


   
            };

            window.Deactivated += (s, e) =>
            {
                Log.Debug("Deactivated");
            };
            window.Stopped += (s, e) =>
            {
                Log.Debug("Stopped");
            };
            window.Destroying += (s, e) =>
            {

                RideRequestService.StopService();
                
                Log.Debug("Destroying");
            };
            return window;
        }





        public async void Setup()
        {

        }





        public async Task ConnectWebsocket()
        {
            Log.Debug("Connecting Websockets");
            Uri uri = new Uri("wss://www.trevorsrides.com/wss/Rider");
            ClientWebSocket client = new ClientWebSocket();
            CancellationTokenSource cts = new CancellationTokenSource();


            try
            {
                await client.ConnectAsync(uri, cts.Token).ConfigureAwait(false);
                Log.Debug("Connected Websocket");
            }
            catch (WebSocketException ex)
            {
                Log.Debug("WebsocketException", ex.Message);
                Log.Debug("WebsocketException", ex.WebSocketErrorCode.ToString());
                Log.Debug("WebsocketException", ex.ErrorCode.ToString());

            }
            catch (Exception ex)
            {
                Log.Debug("Websocket Connection Failed", ex.Message);
                Log.Debug("Websocket Connection Failed", ex.GetType().ToString());
            }

            Log.Debug("Websocket Connected");
            
            
            
            
            while (!cts.IsCancellationRequested)
            {
                byte[] buffer = new byte[1024];
                Log.Debug("Waiting");
                var response = await client.ReceiveAsync(buffer, cts.Token);
                
                string message = System.Text.Encoding.ASCII.GetString(buffer, 0, response.Count);
                Log.Debug("RECEIVED", message);
                
                try
                {
                    Log.Debug("BEfore Desrialize");
                    TrevorsStatus = JsonSerializer.Deserialize<DriverStatus>(message);
                    Log.Debug("AFTER deserialize");
                }
                catch (JsonException ex) 
                {
                    Log.Debug("JsonException", ex.Message);
                }
                catch(NotSupportedException ex)
                {
                    Log.Debug("Not supported Exception", ex.Message);
                }
                catch (ArgumentNullException ex)
                {
                    Log.Debug("Argument Null Exception", ex.Message);
                }
                catch (Exception ex)
                {
                    Log.Debug("Exception", ex.Message);
                    Log.Debug("Exception", ex.GetType().ToString());
                }
                Log.Debug("HELO");
                Log.Debug("IS null", TrevorsStatus == null ? "null" : "Not null");
                try
                {
                    Log.Debug("TREVOR", JsonSerializer.Serialize(TrevorsStatus));
                }
                catch (Exception ex)
                {
                    Log.Debug("Exception", ex.Message);
                    Log.Debug("Exception", ex.GetType().ToString());
                    Log.Debug("Exception", ex.InnerException?.Message ?? "No inner exception");
                    Log.Debug("Stack Trace", ex.StackTrace);
                }



                if ((Application.Current.MainPage as NavigationPage)?.CurrentPage is MainPage)
                {
                    
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        MainPage page = (Application.Current.MainPage as NavigationPage)?.CurrentPage as MainPage;
                        //MainPage page = AppShell.Current.CurrentPage as MainPage;
                        if (TrevorsStatus.isOnline)
                        {
                            page.trevorsStatus.Text = "Trevor is Online!";
                            
                            Log.Debug("TREvor null", TrevorsStatus.latitude.HasValue.ToString());
                            try
                            {
                                TrevorsLocation.Location = new Location(TrevorsStatus.latitude!.Value, TrevorsStatus.longitude!.Value);
                            }
                            catch (Exception ex)
                            {
                                Log.Debug("Exception", ex.Message);
                                Log.Debug("Exception", ex.GetType().ToString());
                                Log.Debug("Exception", ex.InnerException?.Message ?? "No inner exception");
                                Log.Debug("Stack Trace", ex.StackTrace ?? "No stack trace");
                            }
                            
                            
                            if (!page.Map.Pins.Contains(TrevorsLocation))
                            {
                                Log.Debug("2");
                                try
                                {
                                    page.Map.Pins.Add(TrevorsLocation);
                                }
                                catch(Exception ex)
                                {
                                    Log.Debug("Exception", ex.Message);
                                    Log.Debug("Exception", ex.GetType().ToString());
                                    Log.Debug("Exception", ex.InnerException?.Message ?? "No inner exception");
                                    Log.Debug("Stack Trace", ex.StackTrace ?? "No stack trace");
                                }
                                
                                Log.Debug("3");
                                TrevorsLocation.Location = new MauiLocation.Location(TrevorsStatus.latitude!.Value, TrevorsStatus.longitude!.Value);
                                Log.Debug("Finished");
                            }
                        }
                        else
                        {
                            page.trevorsStatus.Text = "Trevor is offline :(";

                            if (page.Map.Pins.Contains(TrevorsLocation))
                            {
                                page.Map.Pins.Remove(TrevorsLocation);
                                
                            }
                        }

                    });
                    
                }
            }
            
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