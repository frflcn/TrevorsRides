using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace TrevorsRidesHelpers
{
    public class VerificationCode
    {
        public int Code { get; }
        public DateTimeOffset Issued { get; }
        public DateTimeOffset Expiry { get; }
        public bool IsVerified { get; private set; }

        public VerificationCode()
        {
            Code = RandomNumberGenerator.GetInt32(100000, 1000000);
            Issued = DateTimeOffset.UtcNow;
            Expiry = DateTimeOffset.UtcNow.AddMinutes(15);
            IsVerified = false;
        }
        public VerificationCode(string value)
        {
            string[] values = value.Split(" - ");
            Code = int.Parse(values[0]);
            Issued = DateTimeOffset.Parse(values[1]);
            Expiry = DateTimeOffset.Parse(values[2]);
            IsVerified = bool.Parse(values[3]);
        }

        public bool IsExpired()
        {
            if (Expiry < DateTimeOffset.UtcNow)
            {
                return true;
            }
            else return false;
        }

        public bool Verify(int code)
        {
            if (IsExpired())
            {
                return false;
            }
            if (IsVerified)
            {
                return false;
            }
            if (code == Code)
            {
                IsVerified = true;
                return true;
            }
            else return false;
        }

    }
}
