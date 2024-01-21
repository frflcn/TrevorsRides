using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Org.BouncyCastle.Utilities.Zlib;
using PhoneNumbers;
using System.Reflection.Metadata;
using System.Text.Json;
using TrevorsRidesHelpers;
using TrevorsRidesHelpers.Ride;

namespace TrevorsRidesServer.Models
{
    public class RidesModel : DbContext
    {
        public static PhoneNumberUtil PhoneNumberUtil { get; set; }


        public DbSet<DriverAccountSetupEntry> DriverAccountSetups { get; set; }
        public DbSet<DriverAccountEntry> DriverAccounts { get; set; }
        public DbSet<RiderAccountSetupEntry> RiderAccountSetups { get; set; }
        public DbSet<RiderAccountEntry> RiderAccounts { get; set; }
        public DbSet<Ride> CompletedRides { get; set; }
        public DbSet<RideInProgress> RidesInProgress { get; set; }
        //public DbSet<RiderAccountEntry> DriverAccounts { get; set; }

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
                DbPath = $"{Helpers.DataFolder}RidesModel.db";
            }
            else
            {
                DbPath = "Ooops";
            }

            
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder
                .Entity<RideInProgress>()
                .HasIndex(e => e.RiderID)
                .IsUnique();
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

            configurationBuilder
                .Properties<Pickup>()
                .HaveConversion<PickupConverter>();
            configurationBuilder
                .Properties<Stop[]>()
                .HaveConversion<StopArrayConverter>();
            configurationBuilder
                .Properties<DropOff>()
                .HaveConversion<DropOffConverter>();
            configurationBuilder
                .Properties<SpaceTimeContinuum>()
                .HaveConversion<SpaceTimeContinuumConverter>();
            configurationBuilder
                .Properties<SpaceTimeUpdateContinuum>()
                .HaveConversion<SpaceTimeUpdateContinuumConverter>();
            configurationBuilder
                .Properties<RideEventUpdateContinuum>()
                .HaveConversion<RideEventUpdateContinuumConverter>();
            configurationBuilder
                .Properties<List<RidePlanUpdate>>()
                .HaveConversion<RidePlanUpdateListConverter>();
            configurationBuilder
                .Properties<RideEventType>()
                .HaveConversion<string>();

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
        public class SpaceTimeContinuumConverter : ValueConverter<SpaceTimeContinuum, byte[]>
        {
            public SpaceTimeContinuumConverter()
                : base(
                      v => v.ToBlob(),
                      v => SpaceTimeContinuum.FromBlob(v))
            { }
        }
        public class SpaceTimeUpdateContinuumConverter : ValueConverter<SpaceTimeUpdateContinuum, byte[]>
        {
            public SpaceTimeUpdateContinuumConverter()
                : base(
                      v => v.ToBlob(),
                      v => SpaceTimeUpdateContinuum.FromBlob(v))
            { }
        }
        public class RideEventUpdateContinuumConverter : ValueConverter<RideEventUpdateContinuum, byte[]>
        {
            public RideEventUpdateContinuumConverter()
                : base(
                      v => v.ToBlob(),
                      v => RideEventUpdateContinuum.FromBlob(v))
            { }
        }
        public class PickupConverter : ValueConverter<Pickup, string>
        {
            public PickupConverter()
                : base(
                      v => JsonSerializer.Serialize(v, Json.Options),
                      v => JsonSerializer.Deserialize<Pickup>(v, Json.Options))
            { }
        }
        public class StopArrayConverter : ValueConverter<Stop[], string>
        {
            public StopArrayConverter()
                : base(
                      v => JsonSerializer.Serialize(v, Json.Options),
                      v => JsonSerializer.Deserialize<Stop[]>(v, Json.Options))
            { }
        }
        public class DropOffConverter : ValueConverter<DropOff, string>
        {
            public DropOffConverter()
                : base(
                      v => JsonSerializer.Serialize(v, Json.Options),
                      v => JsonSerializer.Deserialize<DropOff>(v, Json.Options))
            { }
        }
        public class RidePlanUpdateListConverter : ValueConverter<List<RidePlanUpdate>, string>
        {
            public RidePlanUpdateListConverter()
                : base(
                      v => JsonSerializer.Serialize(v, Json.Options),
                      v => JsonSerializer.Deserialize<List<RidePlanUpdate>>(v, Json.Options))
            { }
        }
        public static string HashedSessionTokenListToString(List<HashedSessionToken> list)
        {

            string result = "";

            foreach (HashedSessionToken token in list)
            {
                result += $"{token} | ";
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
    //TODO: Delete This
    public class Example
    {
        public async void Stuff()
        {
            using(var context = new RidesModel())
            {
                context.Database.GetDbConnection().CreateCommand().ExecuteReader();
            }
            MemoryStream inputStream = new MemoryStream();
            var builder = new SqliteConnectionStringBuilder();
            builder.DataSource = "/var/data/trevorsrides/RidesModel.db";

            var connection = new SqliteConnection(builder.ConnectionString);
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText =
            @"
                INSERT INTO data(value)
                VALUES (zeroblob($length));

                SELECT last_insert_rowid();
            ";
            
            command.Parameters.AddWithValue("$length", inputStream.Length);
            var rowid = (long)command.ExecuteScalar();
            

            using (var writeStream = new SqliteBlob(connection, "data", "value", rowid))
            {
                // NB: Although SQLite doesn't support async, other types of streams do
                await inputStream.CopyToAsync(writeStream);
            }

            using (var writeStream = new SqliteBlob(connection, "RidesInProgress", "RideHistory", rowid))
            {
                writeStream.WriteAsync(new byte[1024], 10, 100);
                await inputStream.CopyToAsync(writeStream);
            }
            var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var dataTable = reader.GetSchemaTable();
               
            }
            
        }
    }
}
