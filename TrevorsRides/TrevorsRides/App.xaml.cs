using System;
using System.Net.WebSockets;
using System.Threading;
using TrevorsRides.Services;
using TrevorsRides.Views;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Timers;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Text;
using System.Runtime.Serialization.Json;
using System.Text.Json;
using Xamarin.Essentials;
using Xamarin.Forms.GoogleMaps;
using System.Collections.Generic;
using System.Net.Http;
using Microsoft.AspNetCore.SignalR.Client;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using Xamarin.Forms.PlatformConfiguration;

namespace TrevorsRides
{
    public partial class App : Application
    {
        CancellationTokenSource cts;
        System.Timers.Timer timer;
        public App()
        {
            InitializeComponent();

            DependencyService.Register<MockDataStore>();
            MainPage = new AppShell();
        }

        protected override async void OnStart()
        {
            HttpClientHandler httpClientHandler = new HttpClientHandler();
            httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => 
            {
                
                Debug.WriteLine("SERVERCERTIFICATECALLBACK");
                return true; 
            };
            Debug.WriteLine("DEBUGGING");
            Console.WriteLine("CONSOLING");
            //await RequestAsync();
            await OpenWebSocket();
            OpenSignalR();
        }

        protected override void OnSleep()
        {
            cts.Cancel();
        }

        protected override async void OnResume()
        {
            Task websocketTask = OpenWebSocket();
        }
        public async Task OpenWebSocket()
        {
            //AndroidClientHandler clientHandler = new AndroidClientHandler();
            //ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(ServerCertificateValidation);
            //ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            using (ClientWebSocket client = new ClientWebSocket())
            {
                client.Options.RemoteCertificateValidationCallback = ServerCertificateValidation;
                Uri uri = new Uri("wss://www.trevorsrides.com/wss/Rider");
                try
                {
                    Console.WriteLine("Connection to WebSocket...");
                    cts = new CancellationTokenSource();
                    await client.ConnectAsync(uri, cts.Token);
                    Console.WriteLine("Connected TO WebSocket");

                    while(!cts.IsCancellationRequested)
                    {
                        var responseBuffer = new byte[1024];
                        ArraySegment<byte> byteToSend = new ArraySegment<byte>(responseBuffer);
                        var response = await client.ReceiveAsync(byteToSend, cts.Token);
                        string message = Encoding.ASCII.GetString(responseBuffer, 0, response.Count);
                        TrevorStatus trevorStatus = JsonSerializer.Deserialize<TrevorStatus>(message);
                        
                        if (AppShell.Current.CurrentPage is RideRequestPage)
                        {
                            RideRequestPage rideRequestPage = AppShell.Current.CurrentPage as RideRequestPage;
                            MainThread.BeginInvokeOnMainThread(() => 
                            {
                                if (trevorStatus.isOnline)
                                {
                                    rideRequestPage.trevorsStatus.Text = "Trevor is online";
                                    rideRequestPage.trevorsLocation.Position = new Position(trevorStatus.latitude, trevorStatus.longitude);
                                    rideRequestPage.trevorsLocation.IsVisible = true;
                                }
                                else
                                {
                                    rideRequestPage.trevorsStatus.Text = "Trevor is offline";
                                    rideRequestPage.trevorsLocation.IsVisible = false;
                                }
                                
                            });
                                
                        }
                        
                    }
                    await client.CloseAsync(WebSocketCloseStatus.NormalClosure, "Close", cts.Token);
                    

                }
                catch (WebSocketException ex)
                {
                    Debug.WriteLine("OOOOOOPPPSS It caught");
                    Debug.WriteLine(ex.Message);
                    Debug.WriteLine(ex.StackTrace);
                    Debug.WriteLine(ex.InnerException);
                    Debug.WriteLine($"HRRESULT: {ex.HResult}");
                    Debug.WriteLine(ex.GetType().ToString());
                    Debug.WriteLine(ex.ErrorCode.ToString());
                    Debug.WriteLine(ex.WebSocketErrorCode.ToString());
                    string message = "";
                    foreach(KeyValuePair<string, string> entry in ex.Data)
                    {
                        message += $"{entry.Key}: {entry.Value}; ";
                    }
                    Debug.WriteLine($"Dictionay: {message}");
                }
            }
        }
        public async void OpenSignalR()
        {
            var connection = new HubConnectionBuilder()
                .WithUrl("wss://www.trevorsrides.com/wss/Rider/")
                .WithAutomaticReconnect()
                .Build();
            try
            {
                await connection.StartAsync();
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine(ex.InnerException.InnerException.ToString());
            }
            
        }
        public void Send()
        {

        }
        public async Task RequestAsync()
        {
            var httpClient = new HttpClient();
            Uri uri = new Uri("https://www.trevorsrides.com/WeatherForecast");
            var response = await httpClient.GetAsync(uri);
            Debug.WriteLine($"Response: {response}");
            Debug.WriteLine($"Response Body: {await response.Content.ReadAsStringAsync()}");
        }
        public bool ServerCertificateValidation(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslErrors)
        {
            Debug.WriteLine("We're here!");
            return true;
        }
    }
}
