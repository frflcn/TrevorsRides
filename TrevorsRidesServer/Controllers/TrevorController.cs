using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Http.Headers;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using TrevorsRidesHelpers;

namespace TrevorsRidesServer.Controllers
{
    [Route("wss/[controller]")]
    public class TrevorController : ControllerBase
    {
        static string testPath = "C://Users/tmsta/AppData/TrevorsRides/trevors_status.json";
        static string trevorStatusFilePath = "/var/data/trevorsrides/trevors_status.json";
        static string riderStatusFilePath = "/var/data/trevorsrides/riders_status.json";
        static string trevorsRidesDirectory = "/var/data/trevorsrides/";
        Random rand = new Random();

        System.Timers.Timer timer = new();

        private readonly ILogger<TrevorController> _logger;

        public TrevorController(ILogger<TrevorController> logger)
        {
            _logger = logger;
        }
        [HttpGet]
        //[HttpGet(Name = "GetTrevor")]
        //[Route("wss/[controller]")]
        public async Task Get()
        {
            _logger.LogInformation("Request To Open: /Trevor");
            if (!System.IO.File.Exists(trevorsRidesDirectory))
            {
                System.IO.Directory.CreateDirectory(trevorsRidesDirectory);
            }

            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                _logger.LogInformation("Its a WebSocket");
                using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();

                await Connect(webSocket);
                
            }
            else
            {
                Log();
                HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                
            }
            string json = System.IO.File.ReadAllText(riderStatusFilePath);
            JsonSerializer.Deserialize<TrevorStatus>(json);
        }
        private async Task Connect(WebSocket webSocket)
        {
            timer.Elapsed += (s, e) => Send(webSocket);
            timer.Interval = 1000;
            timer.Start();
            WebSocketReceiveResult receiveTask;
            do
            {
                string json = "";
                while (true)
                {
                    var buffer = new byte[1024 * 4];
                    receiveTask = await webSocket.ReceiveAsync(
                        new ArraySegment<byte>(buffer), CancellationToken.None);
                    json += Encoding.ASCII.GetString(buffer, 0, receiveTask.Count);
                    if (receiveTask.EndOfMessage)
                    {
                        break;
                    }
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
                        Thread.Sleep(rand.Next(100));
                    }

                }while (!textWritten);
            } while (!receiveTask.CloseStatus.HasValue);
            await webSocket.CloseAsync(receiveTask.CloseStatus.Value, receiveTask.CloseStatusDescription, CancellationToken.None);
            
        }
        private void Send(WebSocket webSocket)
        {
            if (System.IO.File.Exists(riderStatusFilePath))
            {
                try
                {
                    byte[] bytes = System.IO.File.ReadAllBytes(riderStatusFilePath);
                    var sendTask = webSocket.SendAsync(bytes, WebSocketMessageType.Text, true, CancellationToken.None);
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
