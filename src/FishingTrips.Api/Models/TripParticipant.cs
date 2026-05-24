namespace FishingTrips.Api.Models;

public class TripParticipant
{
    public int AnglerId { get; set; }
    public Angler? Angler { get; set; }

    public int FishingTripId { get; set; }
    public FishingTrip? FishingTrip { get; set; }

    public DateTime BookedAt { get; set; } = DateTime.UtcNow;
    public bool Attended { get; set; }
    public decimal? CatchWeightKg { get; set; }
}
