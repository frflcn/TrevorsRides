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
    

    [ApiController]
    public class CreateAccountController : ControllerBase
    {
        PhoneNumberUtil phoneNumberUtil;
        public CreateAccountController() 
        {
            phoneNumberUtil = PhoneNumberUtil.GetInstance();
        }
        [Route("api/Rider/[controller]")]
        [HttpPost]
        public async void OnPost([FromHeader(Name="User-ID")] Guid identifier, [FromHeader(Name="NationalPhoneNumber")] ulong nationalPhoneNumber, [FromHeader(Name="CountryCode")] int countryCode, [FromHeader(Name="Password")] string password)
        {
            PhoneNumber number = phoneNumberUtil.Parse(nationalPhoneNumber.ToString(), phoneNumberUtil.GetRegionCodeForCountryCode(countryCode));

            string hashedPassword = BC.EnhancedHashPassword(password + APIKeys.ServerPepper, 11);

            using (RidesModel context = new RidesModel())
            {
                RiderAccountSetupEntry accountSetup = context.RiderAccountSetups.ToList().Single(e => e.Id == identifier);
                RiderAccountEntry accountEntry;
                if (accountSetup.FirstName == null || accountSetup.LastName == null || accountSetup.Email == null || accountSetup.EmailVerificationCode == null || !accountSetup.EmailVerificationCode.IsVerified)
                {
                    await HttpResponseWritingExtensions.WriteAsync(HttpContext.Response, "The account is not fully formed");
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return;
                }
                if (context.RiderAccounts.ToList().Any(e => e.Id == accountSetup.Id || e.Email == accountSetup.Email || e.PhoneNumber == accountSetup.Phone))
                {
                    await HttpResponseWritingExtensions.WriteAsync(HttpContext.Response, "An account with this Id, Email or Phone Number already exists");
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return;
                }

                accountEntry = new RiderAccountEntry()
                {
                    FirstName = accountSetup.FirstName,
                    LastName = accountSetup.LastName,
                    Email = accountSetup.Email,
                    PhoneNumber = number,
                    Id = identifier,
                    HashedPassword = RiderAccountEntry.HashPassword(password),
                    Status = AccountStatus.Active,
                    RetryCount = new RetryCount(),
                    SessionTokens = new List<HashedSessionToken> {  }
                    
                };
                AccountSession accountSession = accountEntry.ReturnAccountSession();
                
                context.RiderAccounts.Add(accountEntry);

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
        [Route("api/Driver/[controller]")]
        [HttpPost]
        public async void OnDriverPost([FromHeader(Name = "User-ID")] Guid identifier, [FromHeader(Name = "NationalPhoneNumber")] ulong nationalPhoneNumber, [FromHeader(Name = "CountryCode")] int countryCode, [FromHeader(Name = "Password")] string password)
        {
            PhoneNumber number = phoneNumberUtil.Parse(nationalPhoneNumber.ToString(), phoneNumberUtil.GetRegionCodeForCountryCode(countryCode));

            string hashedPassword = BC.EnhancedHashPassword(password + APIKeys.ServerPepper, 11);

            using (RidesModel context = new RidesModel())
            {
                DriverAccountSetupEntry accountSetup = context.DriverAccountSetups.ToList().Single(e => e.Id == identifier);
                DriverAccountEntry accountEntry;
                if (accountSetup.FirstName == null || accountSetup.LastName == null || accountSetup.Email == null || accountSetup.EmailVerificationCode == null || !accountSetup.EmailVerificationCode.IsVerified)
                {
                    await HttpResponseWritingExtensions.WriteAsync(HttpContext.Response, "The account is not fully formed");
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return;
                }
                if (context.DriverAccounts.ToList().Any(e => e.Id == accountSetup.Id || e.Email == accountSetup.Email || e.PhoneNumber == accountSetup.Phone))
                {
                    await HttpResponseWritingExtensions.WriteAsync(HttpContext.Response, "An account with this Id, Email or Phone Number already exists");
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return;
                }

                accountEntry = new DriverAccountEntry()
                {
                    FirstName = accountSetup.FirstName,
                    LastName = accountSetup.LastName,
                    Email = accountSetup.Email,
                    PhoneNumber = number,
                    Id = identifier,
                    HashedPassword = DriverAccountEntry.HashPassword(password),
                    Status = AccountStatus.Active,
                    RetryCount = new RetryCount(),
                    SessionTokens = new List<HashedSessionToken> { }

                };
                AccountSession accountSession = accountEntry.ReturnAccountSession();

                context.DriverAccounts.Add(accountEntry);

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
