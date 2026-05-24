using System.ComponentModel.DataAnnotations;

namespace FishingTrips.Api.Models;

public class Waterbody
{
    public int Id { get; set; }

    [Required, StringLength(120)]
    public string Name { get; set; } = string.Empty;

    public WaterbodyType Type { get; set; }

    [Required, StringLength(200)]
    public string Location { get; set; } = string.Empty;

    [Range(0.01, 100000)]
    public double AreaHa { get; set; }

    [StringLength(500)]
    public string? FishSpecies { get; set; }

    public ICollection<FishingTrip> Trips { get; set; } = new List<FishingTrip>();
}
