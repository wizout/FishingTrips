using FishingTrips.Api.Data;
using FishingTrips.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace FishingTrips.Tests;

public static class TestDbFactory
{
    public static AppDbContext Create(string? name = null)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(name ?? Guid.NewGuid().ToString())
            .EnableSensitiveDataLogging()
            .Options;
        var db = new AppDbContext(options);
        db.Database.EnsureCreated();
        // Strip HasData seed so each test starts clean.
        db.TripParticipants.RemoveRange(db.TripParticipants);
        db.FishingTrips.RemoveRange(db.FishingTrips);
        db.Anglers.RemoveRange(db.Anglers);
        db.Guides.RemoveRange(db.Guides);
        db.Waterbodies.RemoveRange(db.Waterbodies);
        db.SaveChanges();
        return db;
    }

    public static async Task SeedBasicAsync(AppDbContext db)
    {
        db.Anglers.AddRange(
            new Angler { Id = 10, FullName = "A1", Email = "a1@t" },
            new Angler { Id = 11, FullName = "A2", Email = "a2@t" },
            new Angler { Id = 12, FullName = "A3", Email = "a3@t" }
        );
        db.Guides.Add(new Guide { Id = 20, FullName = "G1", LicenseNumber = "L-20", YearsExperience = 5 });
        db.Waterbodies.Add(new Waterbody { Id = 30, Name = "W1", Type = WaterbodyType.Lake, Location = "loc", AreaHa = 5 });
        db.FishingTrips.Add(new FishingTrip
        {
            Id = 40,
            StartAt = new DateTime(2026, 6, 1, 8, 0, 0, DateTimeKind.Utc),
            EndAt = new DateTime(2026, 6, 1, 16, 0, 0, DateTimeKind.Utc),
            MaxParticipants = 2,
            PricePerPerson = 500m,
            WaterbodyId = 30,
            GuideId = 20,
            Status = TripStatus.Planned
        });
        await db.SaveChangesAsync();
    }
}
