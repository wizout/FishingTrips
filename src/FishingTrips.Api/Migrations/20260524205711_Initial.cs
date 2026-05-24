using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace FishingTrips.Api.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Anglers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FullName = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 160, nullable: false),
                    Phone = table.Column<string>(type: "TEXT", maxLength: 32, nullable: true),
                    RegisteredAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Level = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Anglers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Guides",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FullName = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    LicenseNumber = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                    YearsExperience = table.Column<int>(type: "INTEGER", nullable: false),
                    Bio = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Guides", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Waterbodies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    Location = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    AreaHa = table.Column<double>(type: "REAL", nullable: false),
                    FishSpecies = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Waterbodies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FishingTrips",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    StartAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EndAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    MaxParticipants = table.Column<int>(type: "INTEGER", nullable: false),
                    PricePerPerson = table.Column<decimal>(type: "TEXT", precision: 10, scale: 2, nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    WaterbodyId = table.Column<int>(type: "INTEGER", nullable: false),
                    GuideId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FishingTrips", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FishingTrips_Guides_GuideId",
                        column: x => x.GuideId,
                        principalTable: "Guides",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FishingTrips_Waterbodies_WaterbodyId",
                        column: x => x.WaterbodyId,
                        principalTable: "Waterbodies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TripParticipants",
                columns: table => new
                {
                    AnglerId = table.Column<int>(type: "INTEGER", nullable: false),
                    FishingTripId = table.Column<int>(type: "INTEGER", nullable: false),
                    BookedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Attended = table.Column<bool>(type: "INTEGER", nullable: false),
                    CatchWeightKg = table.Column<decimal>(type: "TEXT", precision: 8, scale: 2, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TripParticipants", x => new { x.AnglerId, x.FishingTripId });
                    table.ForeignKey(
                        name: "FK_TripParticipants_Anglers_AnglerId",
                        column: x => x.AnglerId,
                        principalTable: "Anglers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TripParticipants_FishingTrips_FishingTripId",
                        column: x => x.FishingTripId,
                        principalTable: "FishingTrips",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Anglers",
                columns: new[] { "Id", "Email", "FullName", "Level", "Phone", "RegisteredAt" },
                values: new object[,]
                {
                    { 1, "ivan@example.com", "Іван Петренко", 1, "+380501112233", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 2, "olena@example.com", "Олена Коваль", 0, "+380672223344", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 3, "sergii@example.com", "Сергій Мороз", 2, "+380933334455", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.InsertData(
                table: "Guides",
                columns: new[] { "Id", "Bio", "FullName", "LicenseNumber", "YearsExperience" },
                values: new object[,]
                {
                    { 1, "Спеціалізація — щука, судак.", "Микола Гнатюк", "UA-FG-001", 12 },
                    { 2, "Спінінг, сом, нічна рибалка.", "Андрій Шевчук", "UA-FG-002", 7 }
                });

            migrationBuilder.InsertData(
                table: "Waterbodies",
                columns: new[] { "Id", "AreaHa", "FishSpecies", "Location", "Name", "Type" },
                values: new object[,]
                {
                    { 1, 92200.0, "щука, судак, лящ, плітка", "Київська обл.", "Київське водосховище", 3 },
                    { 2, 2750.0, "щука, окунь, лин", "Волинська обл.", "Озеро Світязь", 0 },
                    { 3, 1500.0, "сом, судак, голавль", "Чернігівська обл.", "Річка Десна", 1 }
                });

            migrationBuilder.InsertData(
                table: "FishingTrips",
                columns: new[] { "Id", "EndAt", "GuideId", "MaxParticipants", "PricePerPerson", "StartAt", "Status", "WaterbodyId" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 1, 31, 8, 0, 0, 0, DateTimeKind.Utc), 1, 4, 1200m, new DateTime(2026, 1, 31, 0, 0, 0, 0, DateTimeKind.Utc), 0, 1 },
                    { 2, new DateTime(2026, 2, 15, 10, 0, 0, 0, DateTimeKind.Utc), 2, 3, 1500m, new DateTime(2026, 2, 15, 0, 0, 0, 0, DateTimeKind.Utc), 0, 2 }
                });

            migrationBuilder.InsertData(
                table: "TripParticipants",
                columns: new[] { "AnglerId", "FishingTripId", "Attended", "BookedAt", "CatchWeightKg" },
                values: new object[,]
                {
                    { 1, 1, false, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null },
                    { 2, 1, false, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Anglers_Email",
                table: "Anglers",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FishingTrips_GuideId",
                table: "FishingTrips",
                column: "GuideId");

            migrationBuilder.CreateIndex(
                name: "IX_FishingTrips_WaterbodyId",
                table: "FishingTrips",
                column: "WaterbodyId");

            migrationBuilder.CreateIndex(
                name: "IX_Guides_LicenseNumber",
                table: "Guides",
                column: "LicenseNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TripParticipants_FishingTripId",
                table: "TripParticipants",
                column: "FishingTripId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TripParticipants");

            migrationBuilder.DropTable(
                name: "Anglers");

            migrationBuilder.DropTable(
                name: "FishingTrips");

            migrationBuilder.DropTable(
                name: "Guides");

            migrationBuilder.DropTable(
                name: "Waterbodies");
        }
    }
}
