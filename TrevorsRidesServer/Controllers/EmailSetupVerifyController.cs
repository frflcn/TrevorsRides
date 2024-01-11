using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Requests;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Util;
using Google.Apis.Util.Store;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using TrevorsRidesHelpers;
using TrevorsRidesServer.Models;

namespace TrevorsRidesServer.Controllers
{

    [ApiController]
    public class EmailSetupVerifyController : ControllerBase
    {
        private readonly ILogger<EmailSetupVerifyController> _logger;
        public EmailSetupVerifyController(ILogger<EmailSetupVerifyController> logger)
        {
            _logger = logger;
        }
        [HttpPost]
        [Route("api/Rider/Setup/VerifyEmail")]
        public async Task PostVerifyRiderEmail([FromHeader(Name = "Email")] string email, [FromHeader(Name = "FirstName")] string firstName, [FromHeader(Name = "LastName")] string lastName, [FromHeader(Name = "User-ID")] Guid guid)
        {

            _logger.LogInformation("Post eMail setup verify called");
           

            
            using (RidesModel context = new RidesModel())
            {
                if (context.RiderAccounts.ToList().Any(e => e.Email == email))
                {
                    await HttpResponseWritingExtensions.WriteAsync(HttpContext.Response, "An account with this email already exists");
                    return;
                }
                else
                {
                    RiderAccountSetupEntry verifyEntry = new RiderAccountSetupEntry()
                    {
                        Email = email,
                        EmailVerificationCode = new VerificationCode(),
                        FirstName = firstName,
                        LastName = lastName,
                        Id = guid

                    };
                    if (context.RiderAccountSetups.ToList().Any(e => e.Id == guid))
                    {
                        verifyEntry = context.RiderAccountSetups.ToList().Single(e => e.Id == guid);

                     
                        if (verifyEntry.EmailVerificationCode.Issued.AddMinutes(1) > DateTimeOffset.UtcNow)
                        {
                            string retryTime = context.RiderAccountSetups.Single(e => e.Id == guid).EmailVerificationCode.Issued.AddMinutes(1).ToString();
                            await HttpResponseWritingExtensions.WriteAsync(HttpContext.Response, $"Please wait until: {retryTime} before asking for another email verification code.");
                            return;
                        }
                        else
                        {
                            verifyEntry.Email = email;
                            verifyEntry.FirstName = firstName;
                            verifyEntry.LastName = lastName;
                            verifyEntry.EmailVerificationCode = new VerificationCode();

                            context.RiderAccountSetups.Update(verifyEntry);
                        }
                        
                    }
                    else
                    {
                        context.RiderAccountSetups.Add(verifyEntry);
                    }
                    
                    context.SaveChanges();
                    SendEmail(email, firstName, lastName, verifyEntry.EmailVerificationCode.Code);
                    await HttpResponseWritingExtensions.WriteAsync(HttpContext.Response, "Email Sent");
                    return;
                    
                }
            }
            
        }





        [HttpGet]
        [Route("api/Rider/Setup/VerifyEmail")]
        public async Task GetVerifyRiderEmail([FromHeader(Name = "Email")] string email, [FromHeader(Name = "VerificationCode")] int verificationCode, [FromHeader(Name = "User-ID")] Guid identifier)
        {
            _logger.LogInformation($"Called: Email: {email} Code: {verificationCode} Id: {identifier}");
            using (RidesModel context = new RidesModel())
            {
                if (context.RiderAccountSetups.ToList().Any(e => e.Email == email && e.EmailVerificationCode.Code == verificationCode && e.EmailVerificationCode.Expiry > DateTimeOffset.UtcNow && e.Id == identifier))
                {
                    //context.RiderAccountSetups.ToList().Single(e => e.Email == email && e.EmailVerificationCode.Code == verificationCode).EmailVerificationCode.Verify(verificationCode);
                    RiderAccountSetupEntry accountSetup = context.RiderAccountSetups.ToList().Single(e => e.Email == email && e.EmailVerificationCode.Code == verificationCode);
                    accountSetup.EmailVerificationCode.Verify(verificationCode);

                    context.RiderAccountSetups.Update(accountSetup);
                    context.SaveChanges();
                    await HttpResponseWritingExtensions.WriteAsync(HttpContext.Response, "Success");
                    _logger.LogInformation("Success");

                    return;
                }
                else if (context.RiderAccountSetups.ToList().Any(e => e.Id == identifier && e.EmailVerificationCode.Code == verificationCode))
                {
                    await HttpResponseWritingExtensions.WriteAsync(HttpContext.Response, "Verification Code has expired, please request a new one");
                    _logger.LogInformation("Expired");
                    return;
                }
                else
                {
                    await HttpResponseWritingExtensions.WriteAsync(HttpContext.Response, "Invalid Verification Code");
                    _logger.LogInformation("Invalid");
                    return;
                }
            }
        }
        [HttpPost]
        [Route("api/Driver/Setup/VerifyEmail")]
        public async Task PostVerifyDriverEmail([FromHeader(Name = "Email")] string email, [FromHeader(Name = "FirstName")] string firstName, [FromHeader(Name = "LastName")] string lastName, [FromHeader(Name = "User-ID")] Guid guid)
        {

            _logger.LogInformation("Post eMail setup verify called");



            using (RidesModel context = new RidesModel())
            {
                if (context.DriverAccounts.ToList().Any(e => e.Email == email))
                {
                    await HttpResponseWritingExtensions.WriteAsync(HttpContext.Response, "An account with this email already exists");
                    return;
                }
                else
                {
                    DriverAccountSetupEntry verifyEntry = new DriverAccountSetupEntry()
                    {
                        Email = email,
                        EmailVerificationCode = new VerificationCode(),
                        FirstName = firstName,
                        LastName = lastName,
                        Id = guid

                    };
                    if (context.DriverAccountSetups.ToList().Any(e => e.Id == guid))
                    {
                        verifyEntry = context.DriverAccountSetups.ToList().Single(e => e.Id == guid);


                        if (verifyEntry.EmailVerificationCode.Issued.AddMinutes(1) > DateTimeOffset.UtcNow)
                        {
                            string retryTime = context.DriverAccountSetups.Single(e => e.Id == guid).EmailVerificationCode.Issued.AddMinutes(1).ToString();
                            await HttpResponseWritingExtensions.WriteAsync(HttpContext.Response, $"Please wait until: {retryTime} before asking for another email verification code.");
                            return;
                        }
                        else
                        {
                            verifyEntry.Email = email;
                            verifyEntry.FirstName = firstName;
                            verifyEntry.LastName = lastName;
                            verifyEntry.EmailVerificationCode = new VerificationCode();

                            context.DriverAccountSetups.Update(verifyEntry);
                        }

                    }
                    else
                    {
                        context.DriverAccountSetups.Add(verifyEntry);
                    }

                    context.SaveChanges();
                    SendEmail(email, firstName, lastName, verifyEntry.EmailVerificationCode.Code);
                    await HttpResponseWritingExtensions.WriteAsync(HttpContext.Response, "Email Sent");
                    return;

                }
            }

        }





        [HttpGet]
        [Route("api/Driver/Setup/VerifyEmail")]
        public async Task GetVerifyDriverEmail([FromHeader(Name = "Email")] string email, [FromHeader(Name = "VerificationCode")] int verificationCode, [FromHeader(Name = "User-ID")] Guid identifier)
        {
            using (RidesModel context = new RidesModel())
            {
                if (context.DriverAccountSetups.ToList().Any(e => e.Email == email && e.EmailVerificationCode.Code == verificationCode && e.EmailVerificationCode.Expiry > DateTimeOffset.UtcNow && e.Id == identifier))
                {
                    //context.RiderAccountSetups.ToList().Single(e => e.Email == email && e.EmailVerificationCode.Code == verificationCode).EmailVerificationCode.Verify(verificationCode);
                    DriverAccountSetupEntry accountSetup = context.DriverAccountSetups.ToList().Single(e => e.Email == email && e.EmailVerificationCode.Code == verificationCode);
                    accountSetup.EmailVerificationCode.Verify(verificationCode);

                    context.DriverAccountSetups.Update(accountSetup);
                    context.SaveChanges();
                    await HttpResponseWritingExtensions.WriteAsync(HttpContext.Response, "Success");

                    return;
                }
                else if (context.DriverAccountSetups.ToList().Any(e => e.Id == identifier && e.EmailVerificationCode.Code == verificationCode))
                {
                    await HttpResponseWritingExtensions.WriteAsync(HttpContext.Response, "Verification Code has expired, please request a new one");
                    return;
                }
                else
                {
                    await HttpResponseWritingExtensions.WriteAsync(HttpContext.Response, "Invalid Verification Code");
                    return;
                }
            }
        }





        [ApiExplorerSettings(IgnoreApi = true)]
        protected async void SendEmail(string email, string firstName, string lastName, int verificationCode)
        {
            UserCredential credential;
            string clientSecretFileLocation = "";
            string codeFileLocation = "";
            string fileDataStoreLocation = "";
            GoogleAuthorizationCodeFlow flow;
            
            
            if (OperatingSystem.IsWindows())
            {
                clientSecretFileLocation = "C:/Users/tmsta/OneDrive/Documents/Secrets/client_secret_trevorsrides_email.json";
                codeFileLocation = "C:/Users/tmsta/Onedrive/Documents/Secrets/AdminGoogleEmailCode.txt";
                fileDataStoreLocation = "C:/Users/tmsta/OneDrive/Documents/Secrets/GmailApi";
            }
            else if (OperatingSystem.IsLinux())
            {
                clientSecretFileLocation = "/var/secrets/trevorsrides/client_secret_trevorsrides_email.json";
                codeFileLocation = "/var/secrets/trevorsrides/AdminGoogleEmailCode.txt";
                fileDataStoreLocation = "/var/secrets/trevorsrides/GmailApi";
            }
            using (var stream = new FileStream(clientSecretFileLocation, FileMode.Open, FileAccess.Read))
            {
                flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
                {
                    ClientSecretsStream = stream,
                    Scopes = new string[] { GmailService.Scope.GmailSend },
                    DataStore = new FileDataStore(fileDataStoreLocation)
                });

                _logger.LogInformation($"Home: {Environment.GetEnvironmentVariable("HOME")}");
                _logger.LogInformation($"Username: {Environment.UserName}");
                _logger.LogInformation($"Domain: {Environment.UserDomainName}");
                FileDataStore dataStore = (flow.DataStore as FileDataStore)!;
                TokenResponse response;
                if (null == await dataStore.GetAsync<TokenResponse>("admin"))
                {
                    response = await flow.ExchangeCodeForTokenAsync("admin", System.IO.File.ReadAllText(codeFileLocation), "https://www.trevorsrides.com/Admin", CancellationToken.None);

                }
                else
                {
                    response = await dataStore.GetAsync<TokenResponse>("admin");
                }


                credential = new UserCredential(flow, "admin", response);
                TokenRequest request = new TokenRequest();
                

                _logger.LogInformation($"Token Response: {response.ToString()}");
                
            }


            _logger.LogInformation($"Refresh Token: {credential.Token.RefreshToken}");

            if (credential.Token.IsExpired(SystemClock.Default))
            {
                await credential.RefreshTokenAsync(CancellationToken.None);
            }
            MailMessage mailMessage = new MailMessage();

            mailMessage.From = new MailAddress("admin@trevorsrides.com", "Trevor's Rides");
            mailMessage.To.Add(new MailAddress(email));
            mailMessage.Subject = "Email Verification";
            mailMessage.Body = $"Hello {firstName} {lastName}, here is the verification code you requested: {verificationCode}. Enter this code in the Trevor's Rides app to proceed.";
            mailMessage.ReplyToList.Add(new MailAddress("admin@trevorsrides.com"));
            mailMessage.IsBodyHtml = false;

            MimeMessage mimeMessage = MimeMessage.CreateFromMailMessage(mailMessage);
            Message message = new Message
            {
                Raw = Encode(mimeMessage)
            };


            GmailService service = new GmailService();

            UsersResource.MessagesResource.SendRequest sendRequest = new UsersResource.MessagesResource.SendRequest(service, message, "admin@trevorsrides.com");
            sendRequest.AccessToken = credential.Token.AccessToken;

            sendRequest.Execute();
        }



        protected static string Encode(MimeMessage mimeMessage)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                mimeMessage.WriteTo(ms);
                return Convert.ToBase64String(ms.GetBuffer())
                    .TrimEnd('=')
                    .Replace('+', '-')
                    .Replace('/', '_');
            }
        }
    }
}
