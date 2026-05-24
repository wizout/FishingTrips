using System.ComponentModel.DataAnnotations;

namespace FishingTrips.Api.Models;

public class FishingTrip
{
    public int Id { get; set; }

    [Required]
    public DateTime StartAt { get; set; }

    [Required]
    public DateTime EndAt { get; set; }

    [Range(1, 50)]
    public int MaxParticipants { get; set; }

    [Range(0, 100000)]
    public decimal PricePerPerson { get; set; }

    public TripStatus Status { get; set; } = TripStatus.Planned;

    public int WaterbodyId { get; set; }
    public Waterbody? Waterbody { get; set; }

    public int GuideId { get; set; }
    public Guide? Guide { get; set; }

    public ICollection<TripParticipant> Participants { get; set; } = new List<TripParticipant>();
}
