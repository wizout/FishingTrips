using System.ComponentModel.DataAnnotations;
using FishingTrips.Api.Models;

namespace FishingTrips.Api.DTOs;

public record AnglerDto(int Id, string FullName, string Email, string? Phone, DateTime RegisteredAt, ExperienceLevel Level);

public class AnglerCreateDto
{
    [Required, StringLength(120)] public string FullName { get; set; } = string.Empty;
    [Required, EmailAddress, StringLength(160)] public string Email { get; set; } = string.Empty;
    [Phone, StringLength(32)] public string? Phone { get; set; }
    public ExperienceLevel Level { get; set; } = ExperienceLevel.Beginner;
}

public record GuideDto(int Id, string FullName, string LicenseNumber, int YearsExperience, string? Bio);

public class GuideCreateDto
{
    [Required, StringLength(120)] public string FullName { get; set; } = string.Empty;
    [Required, StringLength(40)] public string LicenseNumber { get; set; } = string.Empty;
    [Range(0, 80)] public int YearsExperience { get; set; }
    [StringLength(2000)] public string? Bio { get; set; }
}

public record WaterbodyDto(int Id, string Name, WaterbodyType Type, string Location, double AreaHa, string? FishSpecies);

public class WaterbodyCreateDto
{
    [Required, StringLength(120)] public string Name { get; set; } = string.Empty;
    public WaterbodyType Type { get; set; }
    [Required, StringLength(200)] public string Location { get; set; } = string.Empty;
    [Range(0.01, 100000)] public double AreaHa { get; set; }
    [StringLength(500)] public string? FishSpecies { get; set; }
}

public record FishingTripDto(
    int Id,
    DateTime StartAt,
    DateTime EndAt,
    int MaxParticipants,
    int BookedCount,
    int FreeSlots,
    decimal PricePerPerson,
    TripStatus Status,
    int WaterbodyId,
    string WaterbodyName,
    int GuideId,
    string GuideName);

public class FishingTripCreateDto
{
    [Required] public DateTime StartAt { get; set; }
    [Required] public DateTime EndAt { get; set; }
    [Range(1, 50)] public int MaxParticipants { get; set; }
    [Range(0, 100000)] public decimal PricePerPerson { get; set; }
    [Required] public int WaterbodyId { get; set; }
    [Required] public int GuideId { get; set; }
}

public record BookingDto(int AnglerId, int FishingTripId, DateTime BookedAt, bool Attended, decimal? CatchWeightKg, string AnglerName);

public class BookingCreateDto
{
    [Required] public int AnglerId { get; set; }
}

public class CompleteTripDto
{
    public Dictionary<int, decimal?> CatchByAnglerId { get; set; } = new();
}
