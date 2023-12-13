using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using BC = BCrypt.Net.BCrypt;
using TrevorsRidesHelpers;

namespace TrevorsRidesServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        [HttpGet]
        public void OnGet()
        {
            string message = "";
            SessionToken sessionToken1 = new SessionToken();
            SessionToken sessionToken2 = new SessionToken();
            HashedSessionToken hashedSessionToken1 = new HashedSessionToken(sessionToken1);
            HashedSessionToken hashedSessionToken1Copy = (HashedSessionToken)hashedSessionToken1.Copy();
            HashedSessionToken hashedSessionToken2 = new HashedSessionToken(sessionToken2);

            message += $"SessionToken1 == SessionToken1: {sessionToken1 == sessionToken1}\n";
            message += $"SessionToken1 == SessionToken2: {sessionToken1 == sessionToken2}\n";
            message += $"HashedSessionToken1 == SessionToken1: {hashedSessionToken1 == sessionToken1}\n";
            message += $"HashedSessionToken1 == SessionToken2: {hashedSessionToken1 == sessionToken2}\n";
            message += $"SessionToken1 == HashedSessionToken1: {sessionToken1 == hashedSessionToken1}\n";
            message += $"SessionToken1 == HashedSessionToken2: {sessionToken1 == hashedSessionToken2}\n";
            message += $"HashedSessionToken1 == HashedSessionToken1: {hashedSessionToken1 == hashedSessionToken1}\n";
            message += $"HashedSessionToken1Copy == HashedSessionToken1: {hashedSessionToken1Copy == hashedSessionToken1}\n";
            message += $"HashedSessionToken1 == HasheddSessionToken2: {hashedSessionToken1 == hashedSessionToken2}\n";
            message += $"HashedSessionToken is SessionToken: {hashedSessionToken1 is SessionToken}\n";
            message += $"(HashedSessionToken as SessionToken) is HashedSessionToken: {(hashedSessionToken1 as SessionToken) is HashedSessionToken}\n";
            message += $"SessionToken is HashedSessionToken: {sessionToken1 is HashedSessionToken} \n";

            string salt = BC.GenerateSalt(4);
            message += $"{BC.HashPassword("Password", salt, true)}\n";
            message += $"{BC.HashPassword("Password5", salt, true)}\n";
            message += $"What?: {Encrypt.Hash("What?")}\n";
            
            var cost = 1;
            var timeTarget = 1000; // Milliseconds
            long timeTaken;
            do
            {
                var sw = Stopwatch.StartNew();
                string password = "What?";
                for (int i = 0; i < Math.Pow(10, cost); i++)
                {
                    password = SessionToken.Hash(password);
                }

                sw.Stop();
                timeTaken = sw.ElapsedMilliseconds;
                message += $"{Math.Pow(10, cost)} hashes took: {timeTaken} milliseconds\n";
                cost += 1;

            } while ((timeTaken) <= timeTarget);





            HttpResponseWritingExtensions.WriteAsync(HttpContext.Response, message);


        }

        
    }
}
