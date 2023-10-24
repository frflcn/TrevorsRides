using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net.WebSockets;
using System.Text.Json;
using System.Text;

namespace TrevorsRidesServer.Controllers
{
    [Route("wss/[controller]")]
    public class RiderController : ControllerBase
    {
        static string testPath = "C://Users/tmsta/AppData/TrevorsRides/trevors_status.json";
        static string trevorStatusFilePath = "/var/www/trevorsrides/trevors_status.json";
        static string riderStatusFilePath = "/var/www/trevorsrides/riders_status.json";
        static string trevorsRidesDirectory = "/var/www/trevorsrides/";
        Random rand = new Random();

        System.Timers.Timer timer = new();
        [HttpGet]
        //[HttpGet(Name = "GetRider")]
        //[Route("wss/[controller]")]
        public async Task Get()
        {
            if (!System.IO.File.Exists(trevorsRidesDirectory))
            {
                System.IO.Directory.CreateDirectory(trevorsRidesDirectory);
            }
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                await Connect(webSocket);

            }
            else
            {
                HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            }
        }
        [NonAction]
        private async Task Connect(WebSocket webSocket)
        {
            timer.Elapsed += (s, e) => Send(webSocket);
            timer.Interval = 1000;
            timer.Start();
            WebSocketReceiveResult receiveResult;
            do
            {
                string json = "";
                while (true)
                {
                    var buffer = new byte[1024 * 4];
                    receiveResult = await webSocket.ReceiveAsync(
                        new ArraySegment<byte>(buffer), CancellationToken.None);

                    json += Encoding.ASCII.GetString(buffer, 0, receiveResult.Count);

                    if (receiveResult.EndOfMessage)
                    {
                        break;
                    }
                    
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
                        Thread.Sleep(rand.Next(100));
                    }
                    
                }       
            } while (!receiveResult.CloseStatus.HasValue);
            await webSocket.CloseAsync(receiveResult.CloseStatus.Value, receiveResult.CloseStatusDescription, CancellationToken.None);

        }
        [NonAction]
        private void Send(WebSocket webSocket)
        {
            if (System.IO.File.Exists(trevorStatusFilePath))
            {
                try
                {
                    byte[] bytes = System.IO.File.ReadAllBytes(trevorStatusFilePath);
                    var sendTask = webSocket.SendAsync(bytes, WebSocketMessageType.Text, true, CancellationToken.None);
                    timer.Interval = 1500 + rand.Next(100);
                }
                catch(Exception ex)
                {
                    Console.WriteLine($"Error reading trevor_status.json");
                    timer.Interval = 1 + rand.Next(100);
                }   
            }
        }
    }
}
