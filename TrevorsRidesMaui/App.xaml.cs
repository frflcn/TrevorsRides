#if (ANDROID || IOS)

using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Timers;
using MauiLocation = Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Controls.Maps;
using TrevorsRidesHelpers;


namespace TrevorsRidesMaui
{
    public partial class App : Application
    {
        public ClientWebSocket client;
        public TrevorStatus? trevor;
        public Pin trevorsLocation = new Pin()
        {
            Label = "Trevor is here",
            Type = PinType.Place
        };
        public string Username { get; set; }
        public string Password { get; set; }
        public App()
        {
            InitializeComponent();
            //Username = await SecureStorage.GetAsync(Username);
            //MainPage = new AppShell();
            MainPage = new LoginPage();
            


        }
        protected override Window CreateWindow(IActivationState? activationState)
        {
            
            Window window = base.CreateWindow(activationState);

            window.Created += async (s, e) =>
            {
                await CheckPermissions();
                Log.Debug("Window Created");
                Task connectTask = ConnectWebsocket();
                
            };

            return window;
        }
        public async Task Setup()
        {

        }
        public async Task ConnectWebsocket()
        {
            Uri uri = new Uri("wss://www.trevorsrides.com/wss/Rider");
            ClientWebSocket client = new ClientWebSocket();
            CancellationTokenSource cts = new CancellationTokenSource();

            try
            {
                await client.ConnectAsync(uri, cts.Token);
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
                    trevor = JsonSerializer.Deserialize<TrevorStatus>(message);
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
                Log.Debug("IS null", trevor == null ? "null" : "Not null");
                try
                {
                    Log.Debug("TREVOR", JsonSerializer.Serialize(trevor));
                }
                catch (Exception ex)
                {
                    Log.Debug("Exception", ex.Message);
                    Log.Debug("Exception", ex.GetType().ToString());
                    Log.Debug("Exception", ex.InnerException?.Message ?? "No inner exception");
                    Log.Debug("Stack Trace", ex.StackTrace);
                }
                


//                
                if (AppShell.Current.CurrentPage is MainPage)
                {
                   
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        MainPage page = AppShell.Current.CurrentPage as MainPage;
                        if (trevor.isOnline)
                        {
                            page.trevorsStatus.Text = "Trevor is Online!";
                            Log.Debug("1");
                            Log.Debug("TREvor null", trevor.latitude.HasValue.ToString());
                            try
                            {
                                trevorsLocation.Location = new Location(trevor.latitude!.Value, trevor.longitude!.Value);
                            }
                            catch (Exception ex)
                            {
                                Log.Debug("Exception", ex.Message);
                                Log.Debug("Exception", ex.GetType().ToString());
                                Log.Debug("Exception", ex.InnerException?.Message ?? "No inner exception");
                                Log.Debug("Stack Trace", ex.StackTrace ?? "No stack trace");
                            }
                            
                            
                            if (!page.Map.Pins.Contains(trevorsLocation))
                            {
                                Log.Debug("2");
                                try
                                {
                                    page.Map.Pins.Add(trevorsLocation);
                                }
                                catch(Exception ex)
                                {
                                    Log.Debug("Exception", ex.Message);
                                    Log.Debug("Exception", ex.GetType().ToString());
                                    Log.Debug("Exception", ex.InnerException?.Message ?? "No inner exception");
                                    Log.Debug("Stack Trace", ex.StackTrace ?? "No stack trace");
                                }
                                
                                Log.Debug("3");
                                trevorsLocation.Location = new MauiLocation.Location(trevor.latitude!.Value, trevor.longitude!.Value);
                                Log.Debug("Finished");
                            }
                        }
                        else
                        {
                            page.trevorsStatus.Text = "Trevor is offline :(";

                            if (page.Map.Pins.Contains(trevorsLocation))
                            {
                                page.Map.Pins.Remove(trevorsLocation);
                                
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