using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrevorsRidesServer.Migrations
{
    /// <inheritdoc />
    public partial class AddedRidesInProgress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Rides");

            migrationBuilder.CreateTable(
                name: "CompletedRides",
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
                    table.PrimaryKey("PK_CompletedRides", x => x.RideId);
                });

            migrationBuilder.CreateTable(
                name: "RidesInProgress",
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
                    table.PrimaryKey("PK_RidesInProgress", x => x.RideId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RidesInProgress_RiderID",
                table: "RidesInProgress",
                column: "RiderID",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CompletedRides");

            migrationBuilder.DropTable(
                name: "RidesInProgress");

            migrationBuilder.CreateTable(
                name: "Rides",
                columns: table => new
                {
                    RideId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CancellationReason = table.Column<string>(type: "TEXT", nullable: true),
                    DriverID = table.Column<Guid>(type: "TEXT", nullable: true),
                    DriversHistory = table.Column<byte[]>(type: "BLOB", nullable: false),
                    DriversHistoryFinalized = table.Column<byte[]>(type: "BLOB", nullable: true),
                    DropOff = table.Column<string>(type: "TEXT", nullable: false),
                    Pickup = table.Column<string>(type: "TEXT", nullable: false),
                    RideEvents = table.Column<byte[]>(type: "BLOB", nullable: false),
                    RidePlanUpdates = table.Column<string>(type: "TEXT", nullable: false),
                    RiderID = table.Column<Guid>(type: "TEXT", nullable: false),
                    RidersHistory = table.Column<byte[]>(type: "BLOB", nullable: false),
                    RidersHistoryFinalized = table.Column<byte[]>(type: "BLOB", nullable: true),
                    Status = table.Column<string>(type: "TEXT", nullable: false),
                    Stops = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rides", x => x.RideId);
                });
        }
    }
}
