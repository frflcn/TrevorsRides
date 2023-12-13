using PhoneNumbers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrevorsRidesHelpers
{
    public class AccountSetup
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string? Email { get; set; }
        public VerificationCode? EmailVerificationCode { get; set; }
        public PhoneNumber? Phone { get; set; }
        public VerificationCode? PhoneVerificationCode { get; set; }
        public string? Password { get; set; }
        public Guid Identifier { get; set; }
        
        public AccountSetup()
        {

        }
        
        public AccountSetup(string firstName, string lastName, Guid identifier)
        {
            FirstName = firstName;
            LastName = lastName;
            Identifier = identifier;
        }
        public AccountSetup(string firstName, string lastName)
        {
            FirstName = firstName;
            LastName = lastName;
            Identifier = Guid.NewGuid();
        }
        
    }
}
