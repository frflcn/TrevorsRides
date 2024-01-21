using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using BC = BCrypt.Net.BCrypt;

namespace TrevorsRidesHelpers
{
    public class SessionToken
    {
        public string Token { get; protected set; }
        public DateTime Expiry { get; protected set; }
        public DateTime Issued { get; protected set; }
        protected bool _IsExpired = false; 
        public bool IsExpired {
            get
            {
                if (_IsExpired == false)
                {
                    if (Expiry < DateTime.UtcNow)
                    {
                        _IsExpired = true;
                        return true;
                    }
                    else
                        return false;
                }
                return _IsExpired;
                
            }
            protected set { _IsExpired = value; }
        }


        protected static char[] CharacterArray = {'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R',
            'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z', 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q',
            'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '`', '~', '!', '@', '#', '$',
            '%', '^', '&', '*', '(', ')', '_', '+', '-', '=', '{', '}', '[', ']', '\\', '|', ';', ':', '\'', '\"', ',', '.', '<', '>', '/', '?' };

        public static SessionToken Parse(string input)
        {
            string[] values = input.Split(" - ");
            return new SessionToken()
            {
                Token = values[0],
                Issued = DateTime.Parse(values[1]),
                Expiry = DateTime.Parse(values[2]),
                _IsExpired = bool.Parse(values[3])
            };

        }

        [JsonConstructor]
        private SessionToken(string token, DateTime expiry, DateTime issued, bool isExpired)
        {
            Token = token;
            Expiry = expiry;
            Issued = issued;
            _IsExpired = isExpired;
        }

        public SessionToken(int minutesAlive = 1440)
        {
            Token = "";
            for (int i = 0; i < 32; i++)
            {
                Token += CharacterArray[RandomNumberGenerator.GetInt32(0, CharacterArray.Length - 1)];
            }

            Issued = DateTime.UtcNow;
            Expiry = DateTime.UtcNow.AddMinutes(minutesAlive);
            
        }

        public void Expire()
        {
            _IsExpired = true;
        }

        public override bool Equals(object? obj)
        {
            if (obj == null) return false;
            if (!(obj is SessionToken)) return false;
            if (obj is RideSessionToken && this is not RideSessionToken) return false;
            if (obj is not RideSessionToken && this is RideSessionToken) return false;

            SessionToken sessionToken = (SessionToken)obj;
            if (sessionToken.Expiry != Expiry) return false;
            if (sessionToken.IsExpired || IsExpired) return false;
            if (this is HashedSessionToken && sessionToken is not HashedSessionToken)
            {
                if (Token != SessionToken.Hash(sessionToken.Token)) return false;
            }
            else if (this is not HashedSessionToken && sessionToken is HashedSessionToken)
            {
                if (SessionToken.Hash(Token) != sessionToken.Token) return false;
            }
            else if (this is HashedRideSessionToken && sessionToken is not HashedRideSessionToken)
            {
                if (Token != SessionToken.Hash(sessionToken.Token)) return false;
            }
            else if (this is not HashedRideSessionToken && sessionToken is HashedRideSessionToken)
            {
                if (SessionToken.Hash(Token) != sessionToken.Token) return false;
            }
            else if (sessionToken.Token != Token) return false;
            return true;
        }

        public static bool operator ==(SessionToken token1, SessionToken token2)
        {
            return token1.Equals(token2);

        }
        public static bool operator !=(SessionToken token1, SessionToken token2)
        {
            return !token1.Equals(token2);
        }
        public override int GetHashCode()
        {
            return Token.GetHashCode();
        }
        public HashedSessionToken ReturnHashedSessionToken()
        {
            return new HashedSessionToken(this);

            
        }
        public object Copy()
        {
            return this.MemberwiseClone();
        }
        public static string Hash(string unhashedToken)
        {
            UnicodeEncoding unicodeEncoding = new UnicodeEncoding();
            byte[] bytes = unicodeEncoding.GetBytes(unhashedToken);
            byte[] hash;
            using (MemoryStream memoryStream = new MemoryStream())
            {
                memoryStream.Write(bytes, 0, bytes.Length);
                SHA384 sha384 = SHA384.Create();
                hash = sha384.ComputeHash(bytes);
            }

            return Convert.ToHexString(hash);
        }
        public override string ToString()
        {
            return $"{Token} - {Issued} - {Expiry} - {IsExpired}";
        }
    }
    public class RideSessionToken : SessionToken
    {
        public static RideSessionToken Parse(string input)
        {
            string[] values = input.Split(" - ");
            return new RideSessionToken()
            {
                Token = values[0],
                Issued = DateTime.Parse(values[1]),
                Expiry = DateTime.Parse(values[2]),   
                _IsExpired = bool.Parse(values[3])
            };

        }


        public RideSessionToken(int minutesAlive = 60)
        {

            for (int i = 0; i < 32; i++)
            {
                Token += CharacterArray[RandomNumberGenerator.GetInt32(0, CharacterArray.Length - 1)];
            }

            Issued = DateTime.UtcNow;
            Expiry = DateTime.UtcNow.AddMinutes(minutesAlive);

        }
    }
    public class HashedSessionToken : SessionToken
    {
        public static HashedSessionToken Parse(string input)
        {
            string[] values = input.Split(" - ");
            SessionToken sessionToken = SessionToken.Parse(input);
            HashedSessionToken hashedSessionToken = new HashedSessionToken(sessionToken);
            hashedSessionToken.Token = values[0];
            return hashedSessionToken;

        }
        public HashedSessionToken(SessionToken sessionToken)
        {
            Token = SessionToken.Hash(sessionToken.Token);
            Expiry = sessionToken.Expiry;
            Issued = sessionToken.Issued;
            _IsExpired = sessionToken.IsExpired;
        }
    }
    public class HashedRideSessionToken : RideSessionToken
    {
        public static HashedRideSessionToken Parse(string input)
        {
            string[] values = input.Split(" - ");
            RideSessionToken rideSessionToken = RideSessionToken.Parse(input);
            HashedRideSessionToken hashedRideSessionToken = new HashedRideSessionToken(rideSessionToken);
            hashedRideSessionToken.Token = values[0];
            return hashedRideSessionToken;

        }
        public HashedRideSessionToken(RideSessionToken rideSessionToken)
        {
            Token = SessionToken.Hash(rideSessionToken.Token);
            Expiry = rideSessionToken.Expiry;
            Issued = rideSessionToken.Issued;
            _IsExpired = rideSessionToken.IsExpired;
        }
    }
    public class Encrypt
    {
        public static string Hash(string password)
        {
            UnicodeEncoding unicodeEncoding = new UnicodeEncoding();
            byte[] bytes = unicodeEncoding.GetBytes(password);
            byte[] hash;
            using (MemoryStream memoryStream = new MemoryStream())
            {
                memoryStream.Write(bytes, 0, bytes.Length);
                SHA384 sha384 = SHA384.Create();
                hash = sha384.ComputeHash(bytes);
            }

            return Convert.ToHexString(hash);
        }
    }
}
