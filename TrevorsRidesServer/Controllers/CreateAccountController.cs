using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PhoneNumbers;
using System.Net;
using System.Text.Json;
using TrevorsRidesHelpers;
using TrevorsRidesServer.Models;
using BC = BCrypt.Net.BCrypt;

namespace TrevorsRidesServer.Controllers
{
    
    [Route("api/[controller]")]
    [ApiController]
    public class CreateAccountController : ControllerBase
    {
        PhoneNumberUtil phoneNumberUtil;
        public CreateAccountController() 
        {
            phoneNumberUtil = PhoneNumberUtil.GetInstance();
        }

        [HttpPost]
        public async void OnPost([FromHeader(Name="Identifier")] Guid identifier, [FromHeader(Name="NationalPhoneNumber")] ulong nationalPhoneNumber, [FromHeader(Name="CountryCode")] int countryCode, [FromHeader(Name="Password")] string password)
        {
            PhoneNumber number = phoneNumberUtil.Parse(nationalPhoneNumber.ToString(), phoneNumberUtil.GetRegionCodeForCountryCode(countryCode));

            string hashedPassword = BC.EnhancedHashPassword(password + APIKeys.ServerPepper, 11);

            using (RidesModel context = new RidesModel())
            {
                AccountSetupEntry accountSetup = context.AccountSetups.ToList().Single(e => e.Identifier == identifier);
                AccountEntry accountEntry;
                if (accountSetup.FirstName == null || accountSetup.LastName == null || accountSetup.Email == null || accountSetup.EmailVerificationCode == null || !accountSetup.EmailVerificationCode.IsVerified)
                {
                    await HttpResponseWritingExtensions.WriteAsync(HttpContext.Response, "The account is not fully formed");
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return;
                }
                if (context.Accounts.ToList().Any(e => e.Id == accountSetup.Identifier || e.Email == accountSetup.Email || e.PhoneNumber == accountSetup.Phone))
                {
                    await HttpResponseWritingExtensions.WriteAsync(HttpContext.Response, "An account with this Identifier, Email or Phone Number already exists");
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return;
                }

                accountEntry = new AccountEntry()
                {
                    FirstName = accountSetup.FirstName,
                    LastName = accountSetup.LastName,
                    Email = accountSetup.Email,
                    PhoneNumber = number,
                    Id = identifier,
                    HashedPassword = AccountEntry.HashPassword(password),
                    Status = AccountStatus.Active,
                    RetryCount = new RetryCount(),
                    SessionTokens = new List<HashedSessionToken> {  }
                    
                };
                AccountSession accountSession = accountEntry.ReturnAccountSession();
                
                context.Accounts.Add(accountEntry);

                context.SaveChanges();
                JsonSerializerOptions jsonOptions = new JsonSerializerOptions
                {
                    Converters =
                {
                    new Json.PhoneNumberJsonConverter()
                }
                };
                await HttpResponseWritingExtensions.WriteAsync(HttpContext.Response, JsonSerializer.Serialize<AccountSession>(accountSession, jsonOptions));
                return;

                    
                
            }
        }
    }
}
