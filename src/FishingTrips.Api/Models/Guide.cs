using System.ComponentModel.DataAnnotations;

namespace FishingTrips.Api.Models;

public class Guide
{
    public int Id { get; set; }

    [Required, StringLength(120)]
    public string FullName { get; set; } = string.Empty;

    [Required, StringLength(40)]
    public string LicenseNumber { get; set; } = string.Empty;

    [Range(0, 80)]
    public int YearsExperience { get; set; }

    [StringLength(2000)]
    public string? Bio { get; set; }

    public ICollection<FishingTrip> LedTrips { get; set; } = new List<FishingTrip>();
}
