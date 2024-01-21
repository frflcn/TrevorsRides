using Microsoft.Maui.Controls.Maps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TrevorsRidesHelpers;
using MauiLocation = Microsoft.Maui.Devices.Sensors;
using TrevorsRidesHelpers.Ride;


namespace TrevorDrivesMaui.BackgroundTasks
{
    public partial class RideRequestService
    {
        public static RideRequestService? Instance { get; set; }
        public static bool IsRunning = false;

        SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);
        ClientWebSocket Client;
        Guid guid;
        System.Timers.Timer timer;
        Random rand = new Random();



        public RideRequestService()
        {
            guid = Guid.NewGuid();
            Log.Debug("RIDE REQUEST SERVICE CONSTRUCTOR", guid.ToString());
        }





        public static partial void StopService();
        public static partial void StartService();





        public async Task Send(string message)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(message);

            await semaphore.WaitAsync();
            try
            {
                await Client.SendAsync(bytes, WebSocketMessageType.Text, false, CancellationToken.None);
            }
            finally
            {
                semaphore.Release();
            }
        }





        public async Task ConnectWebsocketAsync()
        {
            Uri uri = new Uri($"{Helpers.WebsocketDomain}/wss/Driver");
            Client.Options.SetRequestHeader("User-ID", App.AccountSession.Account.Id.ToString());
            Client.Options.SetRequestHeader("SessionToken", App.AccountSession.SessionToken.Token);
            SpaceTime? spaceTime = await GetLocationAsync();
            if (spaceTime == null)
            {
                Log.Debug("RIDE REQUEST SERVICE", "spaceTime is null");
            }
            else
            {
                Log.Debug("RIDE REQUEST SERVICE", "spaceTime is NOT null");
            }
            
            string spaceTimeJson = JsonSerializer.Serialize(spaceTime);
            Client.Options.SetRequestHeader("SpaceTime", spaceTimeJson); //This could cause errors if Getlocation doesn't return properly
            CancellationTokenSource cts = new CancellationTokenSource();
            await Client.ConnectAsync(uri, cts.Token);

            //Debug.WriteLine("YAAAAY");
            System.Timers.Timer timer = new();
            timer.Interval = 1000;
            timer.Elapsed += Send;
            timer.Enabled = true;

            while (!cts.IsCancellationRequested)
            {
                byte[] buffer = new byte[1024 * 64];

                var responseTask = await Client.ReceiveAsync(buffer, cts.Token);

                string message = System.Text.Encoding.ASCII.GetString(buffer, 0, responseTask.Count);
                Log.Debug("MESSGE RECEIVED", message);
                try
                {
                    WebsocketMessage websocketMessage = JsonSerializer.Deserialize<WebsocketMessage>(message);
                    if (websocketMessage.MessageType == MessageType.RideRequest)
                    {
                        HandleRideRequest(websocketMessage);
                    }
                }
                catch(Exception ex)
                {
                    Log.Debug("ERROR", ex.GetType().ToString());
                    Log.Debug("ERROR", ex.Message);
                    Log.Debug("ERROR", ex.StackTrace);
                }
                
                Log.Debug("MESSAGE RECIEVED", "DID WE MAKE IT?");


            }
            async void Send(object sender, EventArgs args)
            {

                try
                {
                    Location? location = await Geolocation.Default.GetLastKnownLocationAsync();
                    if (location == null || (DateTimeOffset.Now.UtcTicks - location.Timestamp.UtcTicks > 5000000)) 
                    {
                        GeolocationRequest request = new GeolocationRequest(GeolocationAccuracy.Best);
                        location = await Geolocation.Default.GetLocationAsync(request);
                    }
                    if (location == null)
                    {
                        Log.Debug("RIDE REQUEST SERVICE", "LOcation Null");
                    }
                    if (App.TrevorStatus == null)
                    {
                        Log.Debug("RIDE REQUEST SERVICE", "TrevorsStatus Null");
                    }

                    App.TrevorStatus.lastKnownLocation = new SpaceTime(new Position(location.Latitude, location.Longitude), location.Timestamp.DateTime);
                    WebsocketMessage websocketMessage = new WebsocketMessage(MessageType.DriverUpdate, App.TrevorStatus);
                    string message = JsonSerializer.Serialize(websocketMessage);
                    ArraySegment<byte> buffer = new ArraySegment<byte>(new byte[1024]);
                    buffer = Encoding.ASCII.GetBytes(message);

                    await Client.SendAsync(buffer, WebSocketMessageType.Text, true, cts.Token);
                    Log.Debug("Message sent: ", message);
                    timer.Interval = 1000 + rand.Next(100);

                }
                catch (FeatureNotSupportedException ex)
                {
                    // Handle not supported on device exception
                    Log.Debug("ERROR", ex.GetType().ToString());
                    Log.Debug("ERROR", ex.Message);
                    Log.Debug("ERROR", ex.StackTrace);
                }
                catch (FeatureNotEnabledException ex)
                {
                    // Handle not enabled on device exception
                    Log.Debug("ERROR", ex.GetType().ToString());
                    Log.Debug("ERROR", ex.Message);
                    Log.Debug("ERROR", ex.StackTrace);
                }
                catch (PermissionException ex)
                {
                    // Handle permission exception
                    Log.Debug("ERROR", ex.GetType().ToString());
                    Log.Debug("ERROR", ex.Message);
                    Log.Debug("ERROR", ex.StackTrace);
                }
                catch (Exception ex)
                {
                    Log.Debug("ERROR", ex.GetType().ToString());
                    Log.Debug("ERROR", ex.Message);
                    Log.Debug("ERROR", ex.StackTrace);
                    // Unable to get location
                }



            }

        }
        /// <summary>
        /// Handle's an incoming ride request
        /// </summary>
        /// <param name="websocketMessage">A websocket message with a MessageType RideRequest</param>
        public void HandleRideRequest(WebsocketMessage websocketMessage)
        {
            if (websocketMessage.MessageType != MessageType.RideRequest)
            {
                throw new ArgumentException("WebsocketMessage Passed to HandleRideRequest should be of type RideRequest");
            }
            MainThread.BeginInvokeOnMainThread(() =>
            {
                (App.Current as App).MainPage = new RideRequestPage(websocketMessage);
            });
            
        }
        public async Task<SpaceTime?> GetLocationAsync()
        {
            try
            {
                Location? location = await Geolocation.Default.GetLastKnownLocationAsync();
                if (location == null || (DateTimeOffset.Now.UtcTicks - location.Timestamp.UtcTicks > 5000000))
                {
                    GeolocationRequest request = new GeolocationRequest(GeolocationAccuracy.Best);
                    location = await Geolocation.Default.GetLocationAsync(request);
                }
                SpaceTime spaceTime = new SpaceTime(new Position(location.Latitude, location.Longitude), location.Timestamp.DateTime);
                App.TrevorStatus.lastKnownLocation = spaceTime;

                return spaceTime;
                

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
            return null;
        }
    }
}
