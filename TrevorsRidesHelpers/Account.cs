using PhoneNumbers;
using System.ComponentModel.DataAnnotations;

namespace TrevorsRidesHelpers
{
    public interface IAccount
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        [Phone]
        public PhoneNumber PhoneNumber { get; set; }
        public AccountStatus Status { get; set; }

        
    }
    public class Account : IAccount
    {
        [Key]
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        [Phone]
        public PhoneNumber PhoneNumber { get; set; }
        public AccountStatus Status { get; set; }


       
    }
    public class AccountSession
    {
        public Account Account { get; set; }
        public SessionToken SessionToken { get; set; }
        public RideSessionToken? RideSessionToken { get; set; }
    }
    public enum AccountStatus
    {
        Active,
        Suspended,
        Deactivated
    }
}
