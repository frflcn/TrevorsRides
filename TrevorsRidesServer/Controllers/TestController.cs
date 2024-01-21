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
        public async Task<IActionResult> OnGet()
        {
            if (Helpers.IsLive)
            {
                return NotFound();
            }


            using(RidesModel context = new RidesModel())
            {
                DriverAccountEntry driver = context.DriverAccounts.Single(e => e.Email == "tmstauff@aol.com");
                RiderAccountEntry rider = context.RiderAccounts.Single(e => e.Email == "tmstauff@aol.com");
                context.DriverAccounts.Remove(driver);
                context.Remove(rider);
                await context.SaveChangesAsync();
            }
            return Ok();


        }

        
    }
}
