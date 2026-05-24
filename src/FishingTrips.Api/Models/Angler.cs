using System.ComponentModel.DataAnnotations;

namespace FishingTrips.Api.Models;

public class Angler
{
    public int Id { get; set; }

    [Required, StringLength(120)]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(160)]
    public string Email { get; set; } = string.Empty;

    [Phone, StringLength(32)]
    public string? Phone { get; set; }

    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;

    public ExperienceLevel Level { get; set; } = ExperienceLevel.Beginner;

    public ICollection<TripParticipant> Participations { get; set; } = new List<TripParticipant>();
}
