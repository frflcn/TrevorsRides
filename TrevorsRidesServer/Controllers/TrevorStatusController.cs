using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.IO;
using System.Diagnostics;
using System.Net.WebSockets;
using System.Timers;
using System.Text;
using TrevorsRidesHelpers;

namespace TrevorsRidesServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TrevorStatusController : ControllerBase
    {
        static string testPath = "C://Users/tmsta/AppData/TrevorsRides/trevors_status.json";
        static string statusFilePath = "/var/data/trevorsrides/trevors_status.json";
        static string statusFileDirectory = "/var/data/trevorsrides/";

        System.Timers.Timer timer = new ();
        [HttpPut(Name = "PutTrevorsStatus")]
        public void Put(bool isOnline, double latitude, double longitude)
        {
            string json = JsonSerializer.Serialize(new DriverStatus(isOnline, new SpaceTime(latitude, longitude, new DateTime(0))));
            if (OperatingSystem.IsWindows())
            {
                
                statusFilePath = testPath;
            }
            System.IO.Directory.CreateDirectory(statusFileDirectory);
            if (!System.IO.File.Exists(statusFilePath)) 
            {
                System.IO.File.Create(statusFilePath);
            }
            System.IO.File.WriteAllText(statusFilePath, json);
        }
        [HttpPost(Name = "PostTrevorsStatus")]
        public void Post(bool isOnline, double latitude, double longitude)
        {
            string json = JsonSerializer.Serialize(new DriverStatus(isOnline, new SpaceTime(latitude, longitude, new DateTime(0))));

            if (OperatingSystem.IsWindows())
            {

                statusFilePath = testPath;
            }
            System.IO.Directory.CreateDirectory(statusFileDirectory);
            if (!System.IO.File.Exists(statusFilePath))
            {
                System.IO.File.Create(statusFilePath);
            }
            System.IO.File.WriteAllText(statusFilePath, json);
        }


    }
}
