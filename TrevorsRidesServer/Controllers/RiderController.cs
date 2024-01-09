using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net.WebSockets;
using System.Text.Json;
using System.Text;
using TrevorsRidesHelpers;
using TrevorsRidesServer.Models;

namespace TrevorsRidesServer.Controllers
{
    [Route("wss/[controller]")]
    public class RiderController : ControllerBase
    {
        SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);
        ILogger<RiderController> _logger;
        static string testPath = "C://Users/tmsta/AppData/TrevorsRides/trevors_status.json";
        static string trevorStatusFilePath = "/var/data/trevorsrides/trevors_status.json";
        static string riderStatusFilePath = "/var/data/trevorsrides/riders_status.json";
        static string trevorsRidesDirectory = "/var/data/trevorsrides/";
        Random rand = new Random();
        WebSocket? websocket;
        RideMatchingService _rideMatchingService { get; set; }

        [FromHeader(Name="User-ID")]
        public Guid? userId { get; set; }
        [FromHeader(Name="SessionToken")]
        public string? sessionToken { get; set; }

        public RiderController(ILogger<RiderController> logger, RideMatchingService rideMatchingService)
        {
            _logger = logger;
            _rideMatchingService = rideMatchingService;
        }

        ~RiderController()
        {
            if (websocket != null)
            {
                websocket.Dispose();
            }
        }
        System.Timers.Timer timer = new();
        [HttpGet]
        public async Task Get()
        {
            _logger.LogDebug("RiderController OnGet");
            //This was going to be the old method of communication should delete soon
            if (!System.IO.File.Exists(trevorsRidesDirectory))
            {
                Directory.CreateDirectory(trevorsRidesDirectory);
            }


            //Make sure rider is requesting websocket
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                //Make sure User-Id and SessionToken are set
                if (userId == null || sessionToken == null)
                {
                    _logger.LogDebug("User-ID and SessionToken must be set");
                    HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                    await HttpResponseWritingExtensions.WriteAsync(HttpContext.Response, "User-ID and SessionToken must be set");
                    return;
                }



                //Validate credentials
                using (RidesModel context = new RidesModel())
                {
                    AccountEntry account;
                    try
                    {
                        account = context.Accounts.Single(e => e.Id == userId);
                    }
                    catch(InvalidOperationException ex)
                    {
                        _logger.LogDebug("User-ID invalid");
                        HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                        await HttpResponseWritingExtensions.WriteAsync(HttpContext.Response, "Invalid credentials");
                        return;
                    }
                    if (!account.VerifySessionToken(sessionToken))
                    {
                        _logger.LogDebug("Sessiontoken invalid");
                        HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                        await HttpResponseWritingExtensions.WriteAsync(HttpContext.Response, "Invalid credentials");
                        return;
                    }
                    
                }
                
                
                //Register Rider if and only if this is the only websocket connection for the rider
                if (!_rideMatchingService.TryRegisterRider(userId.Value, new Rider(Send)))
                {
                    _logger.LogDebug("More than one Websocket Connection");
                    HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                    await HttpResponseWritingExtensions.WriteAsync(HttpContext.Response, "Only one websocket connection per rider is allowed");
                    return;
                }


                //Setup websocket connection
                _logger.LogDebug("Websocket Connected");
                websocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                await Connect();
                
            }
            else //Rider did not request a websocket connection
            {
                _logger.LogDebug("Did you request a websocket?");
                HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                await HttpResponseWritingExtensions.WriteAsync(HttpContext.Response, "Only websocket connections are allowed");
            }
        }




        //Setup websocket connection
        [NonAction]
        private async Task Connect()
        {
            
            timer.Elapsed += (s, e) => Send();
            timer.Interval = 1000;
            timer.Start();
            WebSocketReceiveResult receiveResult;
            do
            {
                string json = "";

                var buffer = new byte[1024 * 4];

                receiveResult = await websocket.ReceiveAsync(
                    new ArraySegment<byte>(buffer), CancellationToken.None);

                json += Encoding.ASCII.GetString(buffer, 0, receiveResult.Count);
                try
                {
                    WebsocketMessage webSocketMessage = JsonSerializer.Deserialize<WebsocketMessage>(json, Json.Options);
                }
                catch(JsonException ex)
                {
                    _logger.LogError($"CloseStatus: {receiveResult.CloseStatus}");
                    _rideMatchingService.DeRegisterRider(userId!.Value);
                    timer.Stop();
                }

                bool textWritten = false;
                while (!textWritten)
                {
                    try
                    {
                        System.IO.File.WriteAllText(riderStatusFilePath, json);
                        textWritten = true;
                    }
                    catch(Exception ex)
                    {
                        Console.WriteLine($"{DateTime.Now}: Error Writing to rider_status.json: {ex.Message}");
                        await Task.Delay(rand.Next(100));
                    }
                    
                }       
            } while (!receiveResult.CloseStatus.HasValue);
            _rideMatchingService.DeRegisterRider(userId!.Value);
            timer.Stop();
            await websocket.CloseAsync(receiveResult.CloseStatus.Value, receiveResult.CloseStatusDescription, CancellationToken.None);
            websocket.Dispose();

        }
        [NonAction]
        private async void Send()
        {
            if (System.IO.File.Exists(trevorStatusFilePath))
            {
                try
                {
                    string trevorStatusString = System.IO.File.ReadAllText(trevorStatusFilePath);
                    DriverStatus trevorStatus = JsonSerializer.Deserialize<DriverStatus>(trevorStatusString);
                    WebsocketMessage websocketMessage = new WebsocketMessage(MessageType.DriverUpdate, trevorStatus);
                    string jsonMessage = JsonSerializer.Serialize(websocketMessage);
                    byte[] bytesToSend = Encoding.UTF8.GetBytes(jsonMessage);

                    await semaphore.WaitAsync();
                    try
                    {
                        await websocket.SendAsync(bytesToSend, WebSocketMessageType.Text, true, CancellationToken.None);
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                        

                    

                    timer.Interval = 1500 + rand.Next(100);
                }
                catch(WebSocketException ex)
                {
                    Console.WriteLine(ex.Message);
                    Console.WriteLine(ex.StackTrace);

                    timer.Stop();
                    _rideMatchingService.DeRegisterRider(userId!.Value);
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.GetType().Name);
                    Console.WriteLine(ex.Message);
                    Console.WriteLine(ex.StackTrace);
                    Console.WriteLine($"Error reading trevor_status.json");

                    timer.Stop();
                    _rideMatchingService.DeRegisterRider(userId!.Value);
                }   
            }
        }
        [NonAction]
        public async Task Send(string message)
        {

            try
            {
                byte[] bytesToSend = Encoding.UTF8.GetBytes(message);

                await semaphore.WaitAsync();
                try
                {
                    await websocket.SendAsync(bytesToSend, WebSocketMessageType.Text, true, CancellationToken.None);
                }
                finally
                {
                    semaphore.Release();
                }


            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
            
        }
    }
}
