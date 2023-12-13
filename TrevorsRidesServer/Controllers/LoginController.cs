using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using TrevorsRidesServer.Models;
using TrevorsRidesHelpers;
using System.Net;
using BC = BCrypt.Net.BCrypt;

namespace TrevorsRidesServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        
        [HttpGet]
        public async void OnGet([FromHeader(Name="Email")] string email, [FromHeader(Name="Password")] string password)
        {
            using (RidesModel context = new RidesModel())
            {
                JsonSerializerOptions jsonOptions = new JsonSerializerOptions
                {
                    Converters =
                {
                    new Json.PhoneNumberJsonConverter()
                }
                };
                AccountEntry account;
                try
                {
                    account = context.Accounts.Single(e => e.Email == email);
                }
                catch(InvalidOperationException)
                {
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    await HttpResponseWritingExtensions.WriteAsync(HttpContext.Response, "Email or password is incorrect");
                    return;
                }
                account.RetryCount.TryReset();
                if (account.RetryCount.Retry())
                {
                    
                    if (account.VerifyPassword(password))
                    {
                        AccountSession accountSession = account.ReturnAccountSession();
                        
                        await HttpResponseWritingExtensions.WriteAsync(HttpContext.Response, JsonSerializer.Serialize<AccountSession>(accountSession, jsonOptions));
                    }
                    else
                    {
                        Response.StatusCode = (int)HttpStatusCode.BadRequest;
                        await HttpResponseWritingExtensions.WriteAsync(HttpContext.Response, "Email or password is incorrect");

                    }
                }
                else
                {
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    await HttpResponseWritingExtensions.WriteAsync(HttpContext.Response, $"Too many attempts, please wait {(account.RetryCount.NextReset-DateTime.UtcNow).TotalMinutes} minutes before attempting again");
                }

                context.Accounts.Update(account);
                context.SaveChanges();
                return;
            }
        }
    }
}
