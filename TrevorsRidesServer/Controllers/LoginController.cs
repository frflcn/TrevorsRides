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
        [FromHeader(Name = "Email")]
        public string? email { get; set; }
        [FromHeader(Name = "User-ID")]
        public Guid? userId { get; set; }
        [FromHeader(Name = "Password")]
        public string? password { get; set; }
        [FromHeader(Name = "SessionToken")]
        public string? sessionToken { get; set; }
        
        
        
        [HttpGet]
        public async void OnGet()
        {
            if (email == null && userId == null)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                await HttpResponseWritingExtensions.WriteAsync(HttpContext.Response, "Email or User-ID must be set");
                return;
            }
            if (password == null && sessionToken == null)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                await HttpResponseWritingExtensions.WriteAsync(HttpContext.Response, "Password or SessionToken must be set");
                return;
            }


            using (RidesModel context = new RidesModel())
            {
                JsonSerializerOptions jsonOptions = new JsonSerializerOptions
                {
                    Converters =
                {
                    new Json.PhoneNumberJsonConverter()
                }
                };

                RiderAccountEntry account;


                try
                {
                    if (userId != null)
                    {
                        account = context.RiderAccounts.ToList().Single(e => e.Id == userId);
                    }
                    else
                    {
                        account = context.RiderAccounts.ToList().Single(e => e.Email == email);
                    }
                    
                }
                catch(InvalidOperationException)
                {
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    await HttpResponseWritingExtensions.WriteAsync(HttpContext.Response, "Invalid Credentials");
                    return;
                }


                account.RetryCount.TryReset();
                if (account.RetryCount.Retry())
                {
                    if (password != null)
                    {
                        if (account.VerifyPassword(password))
                        {
                            AccountSession accountSession = account.ReturnAccountSession();
                            await HttpResponseWritingExtensions.WriteAsync(HttpContext.Response, JsonSerializer.Serialize<AccountSession>(accountSession, jsonOptions));
                        }
                        else
                        {
                            Response.StatusCode = (int)HttpStatusCode.BadRequest;
                            await HttpResponseWritingExtensions.WriteAsync(HttpContext.Response, "Invalid Credentials");
                        }
                    }
                    else
                    {
                        if (account.VerifySessionToken(sessionToken!))
                        {
                            AccountSession accountSession = account.ReturnAccountSession();
                            await HttpResponseWritingExtensions.WriteAsync(HttpContext.Response, JsonSerializer.Serialize<AccountSession>(accountSession, jsonOptions));
                        }
                        else
                        {
                            Response.StatusCode = (int)HttpStatusCode.BadRequest;
                            await HttpResponseWritingExtensions.WriteAsync(HttpContext.Response, "Invalid Credentials");
                        }
                    }
                    
                }
                else
                {
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    await HttpResponseWritingExtensions.WriteAsync(HttpContext.Response, $"Too many attempts, please wait {(account.RetryCount.NextReset-DateTime.UtcNow).TotalMinutes} minutes before attempting again");
                }


                context.RiderAccounts.Update(account);
                context.SaveChanges();
                return;
            }        
        }
    }
}
