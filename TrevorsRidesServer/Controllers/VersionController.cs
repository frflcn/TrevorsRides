using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using TrevorsRidesHelpers;
using TSH = TrevorsRidesHelpers;

namespace TrevorsRidesServer.Controllers
{
    [Route("api/Version")]
    [ApiController]
    public class VersionController : ControllerBase
    {
        public VersionControl DriverVersionControl { get; set; }
        public VersionControl RiderVersionControl { get; set; }
        public VersionController() 
        {
            DriverVersionControl = new VersionControl
            { 
                MinimumVersion = new Version(0, 0, 1),
                RecommendedVersion = new Version(0, 0, 1),
                LatestVersion = new Version(0, 0, 1)
            };
            RiderVersionControl = new VersionControl
            {
                MinimumVersion = new Version(0, 0, 1),
                RecommendedVersion = new Version(0, 0, 1),
                LatestVersion = new Version(0, 0, 1)
            };
        }
        [Route("Driver")]
        [HttpGet()]
        public async Task OnGetDriver()
        {
            string json = JsonSerializer.Serialize<VersionControl>(DriverVersionControl);
            await HttpResponseWritingExtensions.WriteAsync(HttpContext.Response, json);
            return;
        }
        [Route("Rider")]
        [HttpGet()]
        public async Task OnGetRider()
        {
            string json = JsonSerializer.Serialize<VersionControl>(RiderVersionControl);
            await HttpResponseWritingExtensions.WriteAsync(HttpContext.Response, json);
            return;
        }
    }
}
