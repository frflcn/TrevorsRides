using PhoneNumbers;
using System.ComponentModel.DataAnnotations;
using TrevorsRidesHelpers;

namespace TrevorsRidesServer.Models
{


    public class DriverAccountSetupEntry
    {
        [Key]
        public Guid Id { get; set; }
        [EmailAddress]
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public VerificationCode EmailVerificationCode { get; set; }
        [Phone]
        public PhoneNumber? Phone { get; set; }
        public VerificationCode? PhoneVerificationCode { get; set; }
        public string? Password { get; set; }
    }
}
