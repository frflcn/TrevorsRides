using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using BC = BCrypt.Net.BCrypt;
using TrevorsRidesHelpers;
using TrevorsRidesServer.Models;
using TrevorsRidesHelpers.Ride;

namespace TrevorsRidesServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        [HttpGet]
        public async Task OnGet()
        {
            using(RidesModel context = new RidesModel())
            {
                RideInProgress[] rides = context.RidesInProgress.ToArray();
                context.RidesInProgress.RemoveRange(rides);
                await context.SaveChangesAsync();
            }


        }

        
    }
}
