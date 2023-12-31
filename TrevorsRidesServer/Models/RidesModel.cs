﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PhoneNumbers;
using TrevorsRidesHelpers;

namespace TrevorsRidesServer.Models
{
    public class RidesModel : DbContext
    {
        public static PhoneNumberUtil PhoneNumberUtil { get; set; }

        public DbSet<AccountSetupEntry> AccountSetups { get; set; }
        public DbSet<AccountEntry> Accounts { get; set; }
        public string DbPath { get; }

        static RidesModel()
        {
            PhoneNumberUtil = PhoneNumberUtil.GetInstance();
        }

        public RidesModel()
        {
            
            if (OperatingSystem.IsWindows())
            {
                var folder = Environment.SpecialFolder.LocalApplicationData;
                var localPath = Environment.GetFolderPath(folder);
                var appPath = Path.Join(localPath, "TrevorsRides");
                Directory.CreateDirectory(appPath);
                DbPath = Path.Join(appPath, "RidesModel.db");
            }
            else if (OperatingSystem.IsLinux())
            {
                DbPath = "/var/data/trevorsrides/RidesModel.db";
            }
            else
            {
                DbPath = "Ooops";
            }

            
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) => optionsBuilder.UseSqlite($"Data Source={DbPath}");

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            configurationBuilder
                .Properties<VerificationCode>()
                .HaveConversion<VerificationCodeConverter>();
            configurationBuilder
                .Properties<PhoneNumber>()
                .HaveConversion<PhoneNumberConverter>();
            configurationBuilder
                .Properties<RetryCount>()
                .HaveConversion<RetryCountConverter>();
            configurationBuilder
                .Properties<HashedSessionToken>()
                .HaveConversion<HashedSessionTokenConverter>();
            configurationBuilder
                .Properties<HashedRideSessionToken>()
                .HaveConversion<HashedRideSessionTokenConverter>();
            configurationBuilder
                .Properties<List<HashedSessionToken>>()
                .HaveConversion<HashedSessionTokenListConverter>();
            //configurationBuilder
            //    .DefaultTypeMapping<HashedSessionToken>();
        }

        public class VerificationCodeConverter : ValueConverter <VerificationCode, string>
        {
            public VerificationCodeConverter() 
                : base(
                      v => $"{v.Code} - {v.Issued} - {v.Expiry} - {v.IsVerified}",
                      v => new VerificationCode(v))
            { 
            }
        }
        public class PhoneNumberConverter : ValueConverter <PhoneNumber, string>
        {
            public  PhoneNumberConverter()
                : base(
                      v => $"{v.CountryCode} - {v.NationalNumber}",
                      v => StringToPhoneNumber(v))
            { }
        }
        public static PhoneNumber StringToPhoneNumber(string value)
        {
            string[] values = value.Split(" - ");
            return PhoneNumberUtil.Parse(values[1], PhoneNumberUtil.GetRegionCodeForCountryCode(int.Parse(values[0])));
        }
        public class RetryCountConverter : ValueConverter <RetryCount, string>
        {
            public RetryCountConverter()
                : base(
                      v => $"{v.Count} - {v.LastReset}",
                      v => new RetryCount(v))
            { }
        }
        public class HashedSessionTokenConverter : ValueConverter<HashedSessionToken, string>
        {
            public HashedSessionTokenConverter()
                : base(
                      v => $"{v.Token} - {v.Issued} - {v.Expiry} - {v.IsExpired}",
                      v => HashedSessionToken.Parse(v))
            {
            }
        }
        public class HashedRideSessionTokenConverter : ValueConverter<HashedRideSessionToken, string>
        {
            public HashedRideSessionTokenConverter()
                : base(
                      v => $"{v.Token} - {v.Issued} - {v.Expiry} - {v.IsExpired}",
                      v => HashedRideSessionToken.Parse(v))
            {
            }
        }
        public class HashedSessionTokenListConverter : ValueConverter<List<HashedSessionToken>, string>
        {
            public HashedSessionTokenListConverter()
                : base(
                      v => HashedSessionTokenListToString(v),
                      v => StringToHashedSessionTokenList(v))
            { }
        }
        public static string HashedSessionTokenListToString(List<HashedSessionToken> list)
        {

            string result = "";

            foreach (HashedSessionToken token in list)
            {
                result += $"{token.ToString()} | ";
            }
            return result.Substring(0, result.Length - 3);
        }
        public static List<HashedSessionToken> StringToHashedSessionTokenList(string list)
        {
            string[] list2 = list.Split(" | ");
            List<HashedSessionToken> result = new List<HashedSessionToken>(list2.Length);
            foreach (string token in list2)
            {
                result.Add(HashedSessionToken.Parse(token));
            }
            return result;
        }

    }
}
