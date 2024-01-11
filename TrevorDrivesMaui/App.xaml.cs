using System.Net.WebSockets;
using System.Text.Json;
using System.Text;
using TrevorsRidesHelpers;
using TrevorsRidesHelpers.Ride;

using System.Timers;

namespace TrevorDrivesMaui
{
    public partial class App : Application
    {
        public HttpClient HttpClient { get; set; }

        //public static TrevorStatus trevorStatus = new TrevorStatus(false, 40.7338893615268, -77.8858946396202);
        public static DriverStatus trevorStatus = new DriverStatus(false, new SpaceTime(40.7338893615268, -77.8858946396202, new DateTime(0)));
        public static ClientWebSocket client = new ClientWebSocket();
        public Random rand = new Random();

        public App()
        {
            InitializeComponent();
            HttpClient = new HttpClient();
            MainPage = new AppShell();


        }

        protected override Window CreateWindow(IActivationState activationState)
        {
            Window window = base.CreateWindow(activationState);

            window.Created += async (s, e) =>
            {
                await CheckPermissions();
                Task connectTask = ConnectWebsocket();
            };
            Window window1 = new Window(new AppShell());

            return window;
        }
        public async Task ConnectWebsocket()
        {
            Uri uri = new Uri("wss://www.trevorsrides.com/wss/Driver");
            
            CancellationTokenSource cts = new CancellationTokenSource();
            await client.ConnectAsync(uri, cts.Token);

            //Debug.WriteLine("YAAAAY");
            System.Timers.Timer timer = new ();
            timer.Interval = 1000;
            timer.Elapsed += Send;
            timer.Enabled = true;

            while (!cts.IsCancellationRequested)
            {
                byte[] buffer = new byte[1024];

                var responseTask = await client.ReceiveAsync(buffer, cts.Token);

                string message = System.Text.Encoding.ASCII.GetString(buffer, 0, responseTask.Count);
                DriverStatus driverStatus = JsonSerializer.Deserialize<DriverStatus>(message);
                
                if (AppShell.Current.CurrentPage is MainPage)
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        MainPage page = AppShell.Current.CurrentPage as MainPage;
                        
                    });

                }
            }
            async void Send(object sender, EventArgs args)
            {
                
                try
                {
                    Location? location = await Geolocation.Default.GetLastKnownLocationAsync();
                    if (location == null || (DateTimeOffset.Now.UtcTicks - location.Timestamp.UtcTicks > 5000000));
                    {
                        GeolocationRequest request = new GeolocationRequest(GeolocationAccuracy.Best);
                        location = await Geolocation.Default.GetLocationAsync(request);
                    }
                    trevorStatus.lastKnownLocation = new SpaceTime(new Position(location.Latitude, location.Longitude), location.Timestamp.DateTime);
                    WebsocketMessage websocketMessage = new WebsocketMessage(MessageType.DriverUpdate, trevorStatus);
                    string message = JsonSerializer.Serialize(websocketMessage);
                    ArraySegment<byte> buffer = new ArraySegment<byte>(new byte[1024]);
                    buffer = Encoding.ASCII.GetBytes(message);

                    await client.SendAsync(buffer, WebSocketMessageType.Text, true, cts.Token);
                    Log.Debug("Message sent: ", message);
                    timer.Interval = 1000 + rand.Next(100);

                }
                catch (FeatureNotSupportedException fnsEx)
                {
                    // Handle not supported on device exception
                }
                catch (FeatureNotEnabledException fneEx)
                {
                    // Handle not enabled on device exception
                }
                catch (PermissionException pEx)
                {
                    // Handle permission exception
                }
                catch (Exception ex)
                {
                    // Unable to get location
                }
                
                

            }

        }
        public async Task CheckPermissions()
        {
            PermissionStatus locationStatus = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
            if (locationStatus != PermissionStatus.Granted)
            {
                locationStatus = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            }
        }
        
    }
}
