using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Http.Headers;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using TrevorsRidesHelpers;
using TrevorsRidesHelpers.Ride;
using TrevorsRidesServer.Models;

namespace TrevorsRidesServer.Controllers
{
    [Route("wss/[controller]")]
    public class DriverController : ControllerBase
    {
        [FromHeader(Name="User-ID")]
        public Guid? userId { get; set; }
        [FromHeader(Name="SessionToken")]
        public string? sessionToken { get; set; }
        [FromHeader(Name="SpaceTime")]
        public string? spaceTimeJson { get; set; }

        SpaceTime driverSpaceTime { get; set; }
        Dictionary<Guid, TaskCompletionSource<WebsocketMessage>> ReplyListeners { get; set; }

        WebSocket webSocket;
        SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);
        RideMatchingService _rideMatchingService;
        static string testPath = "C://Users/tmsta/AppData/TrevorsRides/trevors_status.json";
        static string trevorStatusFilePath = $"{Helpers.DataFolder}trevors_status.json";
        static string riderStatusFilePath = $"{Helpers.DataFolder}riders_status.json";
        static string trevorsRidesDirectory = $"{Helpers.DataFolder}";
        Random rand = new Random();

        System.Timers.Timer timer = new();

        private readonly ILogger<DriverController> _logger;
        ~DriverController() 
        {
            if (webSocket != null)
            {
                webSocket.Dispose();
            }
        }
        public DriverController(ILogger<DriverController> logger, RideMatchingService rideMatchingService)
        {
            _logger = logger;
            _rideMatchingService = rideMatchingService;
            ReplyListeners = new Dictionary<Guid, TaskCompletionSource<WebsocketMessage>>();
        }
        [HttpGet]
        public async Task Get()
        {
            _logger.LogInformation("Request To Open: /Trevor");
            if (!System.IO.File.Exists(trevorsRidesDirectory))
            {
                System.IO.Directory.CreateDirectory(trevorsRidesDirectory);
            }

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
                //Make sure SpaceTime is set
                if (string.IsNullOrEmpty(spaceTimeJson))
                {
                    _logger.LogDebug("A SpaceTime Must be set");
                    HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                    await HttpResponseWritingExtensions.WriteAsync(HttpContext.Response, "User-ID and SessionToken must be set");
                    return;
                }


                


                //Validate credentials
                using (RidesModel context = new RidesModel())
                {
                    DriverAccountEntry account;
                    try
                    {
                        account = context.DriverAccounts.Single(e => e.Id == userId);
                    }
                    catch (InvalidOperationException ex)
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

                try
                { 

                    _logger.LogDebug(spaceTimeJson);
                    driverSpaceTime = JsonSerializer.Deserialize<SpaceTime>(spaceTimeJson);
                }
                catch (Exception ex)
                {
                    _logger.LogDebug(ex.GetType().ToString());
                    _logger.LogDebug(ex.Message);
                    _logger.LogDebug(ex.StackTrace);
                    _logger.LogDebug("An ill-formed SpaceTime was sent");
                    HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                    await HttpResponseWritingExtensions.WriteAsync(HttpContext.Response, "User-ID and SessionToken must be set");
                    return;
                }

                //Register Driver if and only if this is the only websocket connection for the rider
                DriverStatus driverStatus = new DriverStatus(false, driverSpaceTime);
                if (!_rideMatchingService.TryRegisterDriver(userId.Value, new Driver(Send, AddReplyListener, RemoveReplyListener, driverStatus)))
                {
                    _logger.LogDebug("More than one Websocket Connection");
                    HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                    await HttpResponseWritingExtensions.WriteAsync(HttpContext.Response, "Only one websocket connection per rider is allowed");
                    return;
                }


                //Setup Websocket connection
                _logger.LogDebug("Websocket Connected");
                webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                try
                {
                    await Connect();
                }
                catch (WebSocketException)
                {
                    _rideMatchingService.DeRegisterDriver(userId!.Value);
                    timer.Stop();
                    webSocket.Dispose();
                }
                
                
            }
            else //Driver did not request a websocket connection
            {
                _logger.LogDebug("Did you request a websocket?");
                HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                await HttpResponseWritingExtensions.WriteAsync(HttpContext.Response, "Only websocket connections are allowed");
            }

        }
        public async Task AddReplyListener(Guid messageId, TaskCompletionSource<WebsocketMessage> replyListener)
        {
            ReplyListeners.Add(messageId, replyListener);
        }
        public async Task RemoveReplyListener(Guid messageId)
        {
            TaskCompletionSource<WebsocketMessage> replyListener;
            if (ReplyListeners.TryGetValue(messageId, out replyListener))
            {
                replyListener.TrySetCanceled();
            }
            ReplyListeners.Remove(messageId);
        }
        private async Task Connect()
        {
            int counter = 0;
            timer.Elapsed += (s, e) => Send();
            timer.Interval = 1000;
            timer.Start();
            WebSocketReceiveResult receiveResult;
            DriverStatus driverStatus;
            do
            {
                string json = "";

                var buffer = new byte[1024 * 4];
                
                receiveResult = await webSocket.ReceiveAsync(
                    new ArraySegment<byte>(buffer), CancellationToken.None);




                json += Encoding.ASCII.GetString(buffer, 0, receiveResult.Count);
                if (counter == 7)
                {
                    _logger.LogDebug(json);
                    _logger.LogDebug($"Driver Controller: {userId}");
                    counter = 0;
                }
                counter++;
                try
                {
                    TaskCompletionSource<WebsocketMessage> taskCompletionSource;
                    WebsocketMessage websocketMessage = JsonSerializer.Deserialize<WebsocketMessage>(json, Json.Options);
                    if (websocketMessage.MessageType == MessageType.DriverUpdate)
                    {
                        driverStatus = JsonSerializer.Deserialize<DriverStatus>((JsonElement)websocketMessage.Message);
                        _rideMatchingService.UpdateDriverStatus(userId!.Value, driverStatus);
                    }
                    else if (websocketMessage.MessageType == MessageType.RideRequestAccepted)
                    {
                        if (ReplyListeners.TryGetValue(websocketMessage.MessageID, out taskCompletionSource))
                        {
                            taskCompletionSource.TrySetResult(websocketMessage);
                            ReplyListeners.Remove(websocketMessage.MessageID);
                        }
                    }
                    else if (websocketMessage.MessageType == MessageType.RideRequestDeclined)
                    {
                        if (ReplyListeners.TryGetValue(websocketMessage.MessageID, out taskCompletionSource))
                        {
                            taskCompletionSource.TrySetResult(websocketMessage);
                            ReplyListeners.Remove(websocketMessage.MessageID);
                        }

                    }
                    
                }
                catch(JsonException)
                {
                    _rideMatchingService.DeRegisterDriver(userId!.Value);
                    timer.Stop();
                    webSocket.Dispose();
                }
                catch (WebSocketException)
                {
                    _rideMatchingService.DeRegisterDriver(userId!.Value);
                    timer.Stop();
                    webSocket.Dispose();
                }
                catch(Exception ex)
                {
                    _logger.LogError(ex.GetType().ToString());
                    _logger.LogError(ex.Message);
                    _logger.LogError(ex.StackTrace);
                }
                

                bool textWritten = false;
                do
                {
                    try
                    {
                        System.IO.File.WriteAllText(trevorStatusFilePath, json);
                        textWritten = true;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"{DateTime.Now}: Error writing to trevor_status.json: {ex.Message}");
                        await Task.Delay(rand.Next(100));
 
                    }

                }while (!textWritten);
            } while (!receiveResult.CloseStatus.HasValue);
            _rideMatchingService.DeRegisterDriver(userId!.Value);
            timer.Stop();
            await webSocket.CloseAsync(receiveResult.CloseStatus.Value, receiveResult.CloseStatusDescription, CancellationToken.None);
            webSocket.Dispose();
        }
        private async void Send()
        {

                try
                {
                    byte[] bytes = System.IO.File.ReadAllBytes(riderStatusFilePath);

                    await semaphore.WaitAsync();
                    try
                    {
                        await webSocket.SendAsync(bytes, WebSocketMessageType.Text, false, CancellationToken.None);
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                    
                    timer.Interval = 1000 + rand.Next(100);
                }
                catch(Exception ex)
                {
                    Console.WriteLine($"Error reading ");
                    timer.Stop();
                    _rideMatchingService.DeRegisterDriver(userId!.Value);
                    
                }

            
            
        }
        [NonAction]
        public async Task Send(string message)
        {
            _logger.LogInformation("Driver Send");
            try
            {
                byte[] bytes = Encoding.ASCII.GetBytes(message);

                await semaphore.WaitAsync();
                try
                {
                    await webSocket.SendAsync(bytes, WebSocketMessageType.Text, true, CancellationToken.None);
                }
                finally
                {
                    semaphore.Release();
                }

                timer.Interval = 1000 + rand.Next(100);
            }
            catch(Exception ex) 
            { 
                Console.WriteLine(ex.Message);
                timer.Stop();
                _rideMatchingService.DeRegisterDriver(userId!.Value);

            }
        }
        [NonAction]
        public void Log()
        {
            string message = DateTime.Now.ToString() + " ";
            foreach (var header in Request.Headers)
            {
                message += $"{header.Key}: {header.Value}; ";
            }
            message += $"Path: {Request.Path}; ";
            message += $"Url: {Request.GetDisplayUrl()}";
            _logger.LogInformation(message);
        }
    }
}
