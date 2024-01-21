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
        public async void OnPost([FromHeader(Name="User-ID")] Guid identifier, [FromHeader(Name="NationalPhoneNumber")] ulong nationalPhoneNumber, [FromHeader(Name="CountryCode")] int countryCode, [FromHeader(Name="Password")] string password, [FromHeader(Name="ConfirmPassword")] string? confirmPassword)
        {
            if (confirmPassword != null && confirmPassword != password)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                await HttpResponseWritingExtensions.WriteAsync(HttpContext.Response, "Passwords dont match");
                return;
            }
            if (!password.Any(e => char.IsDigit(e)))
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                await HttpResponseWritingExtensions.WriteAsync(HttpContext.Response, "Password must have atleast one digit");
                return;
            }
            if (!password.Any(e => char.IsLower(e)))
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                await HttpResponseWritingExtensions.WriteAsync(HttpContext.Response, "Password must have atleast one lower case letter");
                return;
            }
            if (!password.Any(e => char.IsUpper(e)))
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                await HttpResponseWritingExtensions.WriteAsync(HttpContext.Response, "Password must have atleast one upper case letter");
                return;
            }
            if (!password.Any(e => char.IsSymbol(e) || char.IsPunctuation(e)))
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                await HttpResponseWritingExtensions.WriteAsync(HttpContext.Response, "Password must have atleast one special letter");
                return;
            }
            if (password.Length < 8)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                await HttpResponseWritingExtensions.WriteAsync(HttpContext.Response, "Password must have atleast eight characters");
                return;
            }

            PhoneNumber number = phoneNumberUtil.Parse(nationalPhoneNumber.ToString(), phoneNumberUtil.GetRegionCodeForCountryCode(countryCode));

            string hashedPassword = BC.EnhancedHashPassword(password + APIKeys.ServerPepper, 11);

            using (RidesModel context = new RidesModel())
            {
                RiderAccountSetupEntry accountSetup = context.RiderAccountSetups.ToList().Single(e => e.Id == identifier);
                RiderAccountEntry accountEntry;
                if (accountSetup.FirstName == null || accountSetup.LastName == null || accountSetup.Email == null || accountSetup.EmailVerificationCode == null || !accountSetup.EmailVerificationCode.IsVerified)
                {
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    await HttpResponseWritingExtensions.WriteAsync(HttpContext.Response, "The account is not fully formed");
                    return;
                }
                if (context.RiderAccounts.ToList().Any(e => e.Id == accountSetup.Id || e.Email == accountSetup.Email || e.PhoneNumber == accountSetup.Phone))
                {
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    await HttpResponseWritingExtensions.WriteAsync(HttpContext.Response, "An account with this Id, Email or Phone Number already exists");
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
        public async void OnDriverPost([FromHeader(Name = "User-ID")] Guid identifier, [FromHeader(Name = "NationalPhoneNumber")] ulong nationalPhoneNumber, [FromHeader(Name = "CountryCode")] int countryCode, [FromHeader(Name = "Password")] string password, [FromHeader(Name = "ConfirmPassword")] string? confirmPassword)
        {
            if (confirmPassword != null && confirmPassword != password)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                await HttpResponseWritingExtensions.WriteAsync(HttpContext.Response, "Passwords dont match");
                return;
            }
            if (!password.Any(e => char.IsDigit(e)))
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                await HttpResponseWritingExtensions.WriteAsync(HttpContext.Response, "Password must have atleast one digit");
                return;
            }
            if (!password.Any(e => char.IsLower(e)))
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                await HttpResponseWritingExtensions.WriteAsync(HttpContext.Response, "Password must have atleast one lower case letter");
                return;
            }
            if (!password.Any(e => char.IsUpper(e)))
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                await HttpResponseWritingExtensions.WriteAsync(HttpContext.Response, "Password must have atleast one upper case letter");
                return;
            }
            if (!password.Any(e => char.IsSymbol(e) || char.IsPunctuation(e)))
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                await HttpResponseWritingExtensions.WriteAsync(HttpContext.Response, "Password must have atleast one special letter");
                return;
            }
            if (password.Length < 8)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                await HttpResponseWritingExtensions.WriteAsync(HttpContext.Response, "Password must have atleast eight characters");
                return;
            }

            PhoneNumber number = phoneNumberUtil.Parse(nationalPhoneNumber.ToString(), phoneNumberUtil.GetRegionCodeForCountryCode(countryCode));

            string hashedPassword = BC.EnhancedHashPassword(password + APIKeys.ServerPepper, 11);

            using (RidesModel context = new RidesModel())
            {
                DriverAccountSetupEntry accountSetup = context.DriverAccountSetups.ToList().Single(e => e.Id == identifier);
                DriverAccountEntry accountEntry;
                if (accountSetup.FirstName == null || accountSetup.LastName == null || accountSetup.Email == null || accountSetup.EmailVerificationCode == null || !accountSetup.EmailVerificationCode.IsVerified)
                {
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    await HttpResponseWritingExtensions.WriteAsync(HttpContext.Response, "The account is not fully formed");
                    return;
                }
                if (context.DriverAccounts.ToList().Any(e => e.Id == accountSetup.Id || e.Email == accountSetup.Email || e.PhoneNumber == accountSetup.Phone))
                {
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    await HttpResponseWritingExtensions.WriteAsync(HttpContext.Response, "An account with this Id, Email or Phone Number already exists");
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
