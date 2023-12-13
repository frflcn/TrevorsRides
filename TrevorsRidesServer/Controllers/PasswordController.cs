using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TrevorsRidesServer.Models;
using TrevorsRidesHelpers;

namespace TrevorsRidesServer.Controllers
{
    [Route("api/Setup")]
    [ApiController]
    public class PasswordController : ControllerBase
    {

        [HttpPost]
        [Route("Password")]
        public async Task PostPassword([FromHeader(Name = "Password")] string password, [FromHeader(Name = "Identifier")] Guid identifier)
        {
            using (RidesModel context = new RidesModel())
            {
                AccountSetupEntry accountSetup = context.AccountSetups.Single(e => e.Identifier == identifier);

                accountSetup.Password = password;

                context.SaveChanges();
            }

        }
    }
}
