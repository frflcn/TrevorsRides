using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrevorsRidesServer.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DriverAccounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    FirstName = table.Column<string>(type: "TEXT", nullable: false),
                    LastName = table.Column<string>(type: "TEXT", nullable: false),
                    Email = table.Column<string>(type: "TEXT", nullable: false),
                    PhoneNumber = table.Column<string>(type: "TEXT", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    HashedPassword = table.Column<string>(type: "TEXT", nullable: false),
                    RetryCount = table.Column<string>(type: "TEXT", nullable: false),
                    SessionTokens = table.Column<string>(type: "TEXT", nullable: false),
                    RideSessionToken = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DriverAccounts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DriverAccountSetups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Email = table.Column<string>(type: "TEXT", nullable: false),
                    FirstName = table.Column<string>(type: "TEXT", nullable: false),
                    LastName = table.Column<string>(type: "TEXT", nullable: false),
                    EmailVerificationCode = table.Column<string>(type: "TEXT", nullable: false),
                    Phone = table.Column<string>(type: "TEXT", nullable: true),
                    PhoneVerificationCode = table.Column<string>(type: "TEXT", nullable: true),
                    Password = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DriverAccountSetups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RiderAccounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    FirstName = table.Column<string>(type: "TEXT", nullable: false),
                    LastName = table.Column<string>(type: "TEXT", nullable: false),
                    Email = table.Column<string>(type: "TEXT", nullable: false),
                    PhoneNumber = table.Column<string>(type: "TEXT", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    HashedPassword = table.Column<string>(type: "TEXT", nullable: false),
                    RetryCount = table.Column<string>(type: "TEXT", nullable: false),
                    SessionTokens = table.Column<string>(type: "TEXT", nullable: false),
                    RideSessionToken = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RiderAccounts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RiderAccountSetups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Email = table.Column<string>(type: "TEXT", nullable: false),
                    FirstName = table.Column<string>(type: "TEXT", nullable: false),
                    LastName = table.Column<string>(type: "TEXT", nullable: false),
                    EmailVerificationCode = table.Column<string>(type: "TEXT", nullable: false),
                    Phone = table.Column<string>(type: "TEXT", nullable: true),
                    PhoneVerificationCode = table.Column<string>(type: "TEXT", nullable: true),
                    Password = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RiderAccountSetups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Rides",
                columns: table => new
                {
                    RideId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Status = table.Column<string>(type: "TEXT", nullable: false),
                    RiderID = table.Column<Guid>(type: "TEXT", nullable: false),
                    DriverID = table.Column<Guid>(type: "TEXT", nullable: true),
                    Pickup = table.Column<string>(type: "TEXT", nullable: false),
                    Stops = table.Column<string>(type: "TEXT", nullable: false),
                    DropOff = table.Column<string>(type: "TEXT", nullable: false),
                    DriversHistoryFinalized = table.Column<byte[]>(type: "BLOB", nullable: true),
                    DriversHistory = table.Column<byte[]>(type: "BLOB", nullable: false),
                    RidersHistory = table.Column<byte[]>(type: "BLOB", nullable: false),
                    RidersHistoryFinalized = table.Column<byte[]>(type: "BLOB", nullable: true),
                    RideEvents = table.Column<byte[]>(type: "BLOB", nullable: false),
                    RidePlanUpdates = table.Column<string>(type: "TEXT", nullable: false),
                    CancellationReason = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rides", x => x.RideId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DriverAccounts");

            migrationBuilder.DropTable(
                name: "DriverAccountSetups");

            migrationBuilder.DropTable(
                name: "RiderAccounts");

            migrationBuilder.DropTable(
                name: "RiderAccountSetups");

            migrationBuilder.DropTable(
                name: "Rides");
        }
    }
}
