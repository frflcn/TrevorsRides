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
using Onion = Maui.GoogleMaps;
using TrevorsRidesHelpers.Ride;

namespace TrevorsRidesMaui.BackgroundTasks
{
    public partial class RideRequestService
    {
        public static RideRequestService? Instance { get; set; }
        public static bool IsRunning = false;

        SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);
        ClientWebSocket Client;
        public DriverStatus? trevorsStatus;
        Guid guid;
        System.Timers.Timer timer;

        public Pin trevorsLocation = new Pin()
        {
            Label = "Trevor is here",
            Type = PinType.Place
        };

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
            Client.Options.SetRequestHeader("User-ID", App.AccountSession.Account.Id.ToString());
            Client.Options.SetRequestHeader("SessionToken", App.AccountSession.SessionToken.Token);
            Log.Debug("Connecting Websockets");
            Uri uri = new Uri($"{Helpers.WebsocketDomain}/wss/Rider");
            
            CancellationTokenSource cts = new CancellationTokenSource();


            //Connect
            try
            {
                await Client.ConnectAsync(uri, cts.Token);
                Log.Debug("Connected Websocket");
            }
            catch (WebSocketException ex)
            {
                Log.Debug("WebsocketException", ex.InnerException?.Message ?? "No Inner exception");
                foreach(var data in ex.Data.Keys)
                {
                    Log.Debug("KEYS", $"{data.ToString()}: {ex.Data[data].ToString()}");
                }
                Log.Debug("WebsocketException", ex.Data.Keys.ToString());
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
                //Log.Debug("Waiting");
                var response = await Client.ReceiveAsync(buffer, cts.Token);
                
                string message = System.Text.Encoding.ASCII.GetString(buffer, 0, response.Count);
                //Log.Debug("RECEIVED", message);

                WebsocketMessage websocketMessage = JsonSerializer.Deserialize<WebsocketMessage>(message);
                if (websocketMessage.MessageType == MessageType.DriverUpdate)
                {
                    DriverStatus driverStatus = JsonSerializer.Deserialize<DriverStatus>((JsonElement)websocketMessage.Message);
                    App.TrevorsStatus = driverStatus;
                }
                
                




                if (((App.Current.MainPage as MyFlyoutPage)?.Detail as NavigationPage)?.CurrentPage is MainPage)
                {

                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        MainPage page = ((App.Current.MainPage as MyFlyoutPage)?.Detail as NavigationPage).CurrentPage as MainPage;
                        //MainPage page = AppShell.Current.CurrentPage as MainPage;
                        if (App.TrevorsStatus.isOnline)
                        {
                            //page.trevorsStatus.Text = "Trevor is Online!";
                            page.Title = "Trevor is Online!";

                            
                            page.TrevorPIN.Position = new Onion.Position(App.TrevorsStatus.latitude!.Value, App.TrevorsStatus.longitude!.Value);
                            if (page.Map.Pins.Contains(page.TrevorPIN))
                            {
                                page.Map.Pins.Remove(page.TrevorPIN);
                            }
                            page.Map.Pins.Add(page.TrevorPIN);
                            //page.Map.Pins[0].

                        }
                        else
                        {
                            //page.trevorsStatus.Text = "Trevor is offline :(";
                            page.Title = "Trevor is offline :(";

                            if (page.Map.Pins.Contains(page.TrevorPIN))
                            {
                                page.Map.Pins.Remove(page.TrevorPIN);

                            }
                        }
                    });

                }
            }

        }
    }
}
