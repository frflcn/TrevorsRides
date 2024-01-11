using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Http.Headers;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using TrevorsRidesHelpers;
using TrevorsRidesHelpers.Ride;

namespace TrevorsRidesServer.Controllers
{
    [Route("wss/[controller]")]
    public class DriverController : ControllerBase
    {
        [FromHeader(Name="User-ID")]
        public Guid? userId { get; set; }
        [FromHeader(Name="SessionToken")]
        public string? sessionToken { get; set; }

        WebSocket webSocket;
        SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);
        RideMatchingService _rideMatchingService;
        static string testPath = "C://Users/tmsta/AppData/TrevorsRides/trevors_status.json";
        static string trevorStatusFilePath = "/var/data/trevorsrides/trevors_status.json";
        static string riderStatusFilePath = "/var/data/trevorsrides/riders_status.json";
        static string trevorsRidesDirectory = "/var/data/trevorsrides/";
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
                webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                _rideMatchingService.TryRegisterDriver(userId.Value, new Driver(Send, new SpaceTime(new Position(43, 34), DateTime.UtcNow)));
                await Connect();
                
            }
            else
            {
                Log();
                HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                
            }
            string json = System.IO.File.ReadAllText(riderStatusFilePath);
            JsonSerializer.Deserialize<DriverStatus>(json);
        }
        private async Task Connect()
        {
            timer.Elapsed += (s, e) => Send();
            timer.Interval = 1000;
            timer.Start();
            WebSocketReceiveResult receiveTask;
            do
            {
                string json = "";

                var buffer = new byte[1024 * 4];

                    receiveTask = await webSocket.ReceiveAsync(
                        new ArraySegment<byte>(buffer), CancellationToken.None);


                json += Encoding.ASCII.GetString(buffer, 0, receiveTask.Count);
                WebsocketMessage websocketMessage = JsonSerializer.Deserialize<WebsocketMessage>(json, Json.Options);
 
                

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
            } while (!receiveTask.CloseStatus.HasValue);
            await webSocket.CloseAsync(receiveTask.CloseStatus.Value, receiveTask.CloseStatusDescription, CancellationToken.None);
            
        }
        private async void Send()
        {
            if (System.IO.File.Exists(riderStatusFilePath))
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
                }
                finally
                {
                    timer.Interval = 1 + rand.Next(100);
                }
            }
            
        }
        [NonAction]
        public async Task Send(string message)
        {
            try
            {
                byte[] bytes = Encoding.ASCII.GetBytes(message);

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
                Console.WriteLine(ex.Message); 
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
