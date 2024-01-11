using PhoneNumbers;
using System.ComponentModel.DataAnnotations;
using TrevorsRidesHelpers;
using TRH = TrevorsRidesHelpers;
using BC = BCrypt.Net.BCrypt;
using BCrypt.Net;

namespace TrevorsRidesServer.Models
{


    public class DriverAccountEntry : IAccount
    {
        [Key]
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        [Phone]
        public PhoneNumber PhoneNumber { get; set; }
        public AccountStatus Status { get; set; }
        public string HashedPassword {  get; set; }
        public RetryCount RetryCount { get; set; }
        public List<HashedSessionToken> SessionTokens { get; set; }
        public HashedRideSessionToken? RideSessionToken { get; set; }
        //public string ServerState { get; set; }




        /// <summary>
        /// Adds a HashedSessionToken to the RiderAccountEntry's Session Tokens then returns an AccountSession with the unhashed Token
        /// </summary>
        /// <returns>An Account Session</returns>
        public AccountSession ReturnAccountSession()
        {
            SessionToken sessionToken = new SessionToken();
            SessionTokens.Add(new HashedSessionToken(sessionToken));
            SessionTokens.RemoveAll(e => e.IsExpired == true);
            return new AccountSession()
            {
                Account = new Account()
                {
                    Id = Id,
                    FirstName = FirstName,
                    LastName = LastName,
                    Email = Email,
                    PhoneNumber = PhoneNumber,
                    Status = Status
                },
                SessionToken = sessionToken
            };
            
            
        }
        public static string HashPassword(string password)
        {
            return BC.EnhancedHashPassword(password + APIKeys.ServerPepper, workFactor: 11);
        }

        public bool VerifyPassword(string password)
        {
            return BC.EnhancedVerify(password + APIKeys.ServerPepper, HashedPassword);
        }

        public bool ReplacePassword(string oldPassword, string newPassword)
        {
            try
            {
                HashedPassword = BC.ValidateAndReplacePassword(oldPassword, HashedPassword, newPassword);
                return true;
            }
            catch(BcryptAuthenticationException ex)
            {
                return false;
            }
            
        }

        public bool VerifySessionToken(string token)
        {
            if (SessionTokens.Exists(e => e.Token == SessionToken.Hash(token) && e.IsExpired == false)) return true;
            else return false;
        }

        public bool VerifySessionToken(SessionToken sessionToken)
        {
            if (sessionToken.IsExpired)
            {
                return false;
            }
            return SessionTokens.Contains(sessionToken);
        }

        public bool VerifyRideSessionToken(SessionToken rideSessionToken)
        {
            if (rideSessionToken.IsExpired)
            {
                return false;
            }
            return rideSessionToken == RideSessionToken;
        }

    }

}
