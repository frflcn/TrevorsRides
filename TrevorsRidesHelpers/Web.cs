using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrevorsRidesHelpers
{
    public class Web
    {
        public static readonly HttpClient HttpClient;
        public static readonly string Domain;
        static Web()
        {
            HttpClient = new HttpClient();
#pragma warning disable CS0162 // Unreachable code detected
            if (Helpers.IsLive)
            {
                
                Domain = "https://www.trevorsrides.com/";
                
            }
            else
            {
                Domain = "https://www.test.trevorsrides.com/";
            }
#pragma warning restore CS0162 // Unreachable code detected
        }
        public class Endpoints
        {

        }
        public class Headers
        {
            public const string Email = "Email";
            public const string Password = "Password";
            public const string VerificationCode = "VerificationCode";
            public const string SessionToken = "SessionToken";
            public const string RiderId = "RiderId";
            public const string DriverId = "DriverId";
            public const string GeneralId = "Id";
            public const string RideId = "RideId";
        }
    }
}
