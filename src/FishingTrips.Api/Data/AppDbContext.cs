using FishingTrips.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace FishingTrips.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Angler> Anglers => Set<Angler>();
    public DbSet<Guide> Guides => Set<Guide>();
    public DbSet<Waterbody> Waterbodies => Set<Waterbody>();
    public DbSet<FishingTrip> FishingTrips => Set<FishingTrip>();
    public DbSet<TripParticipant> TripParticipants => Set<TripParticipant>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Angler>(e =>
        {
            e.HasIndex(a => a.Email).IsUnique();
        });

        b.Entity<Guide>(e =>
        {
            e.HasIndex(g => g.LicenseNumber).IsUnique();
        });

        b.Entity<FishingTrip>(e =>
        {
            e.Property(t => t.PricePerPerson).HasPrecision(10, 2);
            e.HasOne(t => t.Waterbody)
                .WithMany(w => w.Trips)
                .HasForeignKey(t => t.WaterbodyId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(t => t.Guide)
                .WithMany(g => g.LedTrips)
                .HasForeignKey(t => t.GuideId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<TripParticipant>(e =>
        {
            e.HasKey(p => new { p.AnglerId, p.FishingTripId });
            e.Property(p => p.CatchWeightKg).HasPrecision(8, 2);
            e.HasOne(p => p.Angler)
                .WithMany(a => a.Participations)
                .HasForeignKey(p => p.AnglerId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(p => p.FishingTrip)
                .WithMany(t => t.Participants)
                .HasForeignKey(p => p.FishingTripId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        SeedData(b);
    }

    private static void SeedData(ModelBuilder b)
    {
        var seedDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        b.Entity<Angler>().HasData(
            new Angler { Id = 1, FullName = "Іван Петренко", Email = "ivan@example.com", Phone = "+380501112233", RegisteredAt = seedDate, Level = ExperienceLevel.Intermediate },
            new Angler { Id = 2, FullName = "Олена Коваль", Email = "olena@example.com", Phone = "+380672223344", RegisteredAt = seedDate, Level = ExperienceLevel.Beginner },
            new Angler { Id = 3, FullName = "Сергій Мороз", Email = "sergii@example.com", Phone = "+380933334455", RegisteredAt = seedDate, Level = ExperienceLevel.Advanced }
        );

        b.Entity<Guide>().HasData(
            new Guide { Id = 1, FullName = "Микола Гнатюк", LicenseNumber = "UA-FG-001", YearsExperience = 12, Bio = "Спеціалізація — щука, судак." },
            new Guide { Id = 2, FullName = "Андрій Шевчук", LicenseNumber = "UA-FG-002", YearsExperience = 7, Bio = "Спінінг, сом, нічна рибалка." }
        );

        b.Entity<Waterbody>().HasData(
            new Waterbody { Id = 1, Name = "Київське водосховище", Type = WaterbodyType.Reservoir, Location = "Київська обл.", AreaHa = 92200, FishSpecies = "щука, судак, лящ, плітка" },
            new Waterbody { Id = 2, Name = "Озеро Світязь", Type = WaterbodyType.Lake, Location = "Волинська обл.", AreaHa = 2750, FishSpecies = "щука, окунь, лин" },
            new Waterbody { Id = 3, Name = "Річка Десна", Type = WaterbodyType.River, Location = "Чернігівська обл.", AreaHa = 1500, FishSpecies = "сом, судак, голавль" }
        );

        b.Entity<FishingTrip>().HasData(
            new FishingTrip { Id = 1, StartAt = seedDate.AddDays(30), EndAt = seedDate.AddDays(30).AddHours(8), MaxParticipants = 4, PricePerPerson = 1200m, Status = TripStatus.Planned, WaterbodyId = 1, GuideId = 1 },
            new FishingTrip { Id = 2, StartAt = seedDate.AddDays(45), EndAt = seedDate.AddDays(45).AddHours(10), MaxParticipants = 3, PricePerPerson = 1500m, Status = TripStatus.Planned, WaterbodyId = 2, GuideId = 2 }
        );

        b.Entity<TripParticipant>().HasData(
            new TripParticipant { AnglerId = 1, FishingTripId = 1, BookedAt = seedDate, Attended = false },
            new TripParticipant { AnglerId = 2, FishingTripId = 1, BookedAt = seedDate, Attended = false }
        );
    }
}
