using FishingTrips.Api.Data;
using FishingTrips.Api.DTOs;
using FishingTrips.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace FishingTrips.Api.Services;

public class FishingTripService : IFishingTripService
{
    private readonly AppDbContext _db;

    public FishingTripService(AppDbContext db) => _db = db;

    public async Task<IEnumerable<FishingTripDto>> GetAllAsync(int? waterbodyId, int? guideId, DateTime? from, DateTime? to)
    {
        var q = _db.FishingTrips
            .Include(t => t.Waterbody)
            .Include(t => t.Guide)
            .Include(t => t.Participants)
            .AsQueryable();

        if (waterbodyId.HasValue) q = q.Where(t => t.WaterbodyId == waterbodyId.Value);
        if (guideId.HasValue) q = q.Where(t => t.GuideId == guideId.Value);
        if (from.HasValue) q = q.Where(t => t.StartAt >= from.Value);
        if (to.HasValue) q = q.Where(t => t.StartAt <= to.Value);

        var list = await q.OrderBy(t => t.StartAt).ToListAsync();
        return list.Select(ToDto);
    }

    public async Task<FishingTripDto?> GetByIdAsync(int id)
    {
        var t = await _db.FishingTrips
            .Include(x => x.Waterbody)
            .Include(x => x.Guide)
            .Include(x => x.Participants)
            .FirstOrDefaultAsync(x => x.Id == id);
        return t is null ? null : ToDto(t);
    }

    public async Task<ServiceResult<FishingTripDto>> CreateAsync(FishingTripCreateDto dto)
    {
        if (dto.EndAt <= dto.StartAt)
            return ServiceResult<FishingTripDto>.Invalid("EndAt must be after StartAt");

        if (!await _db.Waterbodies.AnyAsync(w => w.Id == dto.WaterbodyId))
            return ServiceResult<FishingTripDto>.NotFound($"Waterbody {dto.WaterbodyId} not found");
        if (!await _db.Guides.AnyAsync(g => g.Id == dto.GuideId))
            return ServiceResult<FishingTripDto>.NotFound($"Guide {dto.GuideId} not found");

        var entity = new FishingTrip
        {
            StartAt = dto.StartAt,
            EndAt = dto.EndAt,
            MaxParticipants = dto.MaxParticipants,
            PricePerPerson = dto.PricePerPerson,
            WaterbodyId = dto.WaterbodyId,
            GuideId = dto.GuideId,
            Status = TripStatus.Planned
        };
        _db.FishingTrips.Add(entity);
        await _db.SaveChangesAsync();

        var created = (await GetByIdAsync(entity.Id))!;
        return ServiceResult<FishingTripDto>.Ok(created);
    }

    public async Task<ServiceResult<FishingTripDto>> UpdateAsync(int id, FishingTripCreateDto dto)
    {
        var entity = await _db.FishingTrips.FindAsync(id);
        if (entity is null) return ServiceResult<FishingTripDto>.NotFound($"Trip {id} not found");
        if (dto.EndAt <= dto.StartAt)
            return ServiceResult<FishingTripDto>.Invalid("EndAt must be after StartAt");
        if (!await _db.Waterbodies.AnyAsync(w => w.Id == dto.WaterbodyId))
            return ServiceResult<FishingTripDto>.NotFound($"Waterbody {dto.WaterbodyId} not found");
        if (!await _db.Guides.AnyAsync(g => g.Id == dto.GuideId))
            return ServiceResult<FishingTripDto>.NotFound($"Guide {dto.GuideId} not found");

        var participantCount = await _db.TripParticipants.CountAsync(p => p.FishingTripId == id);
        if (dto.MaxParticipants < participantCount)
            return ServiceResult<FishingTripDto>.Conflict($"Cannot reduce MaxParticipants below current booking count {participantCount}");

        entity.StartAt = dto.StartAt;
        entity.EndAt = dto.EndAt;
        entity.MaxParticipants = dto.MaxParticipants;
        entity.PricePerPerson = dto.PricePerPerson;
        entity.WaterbodyId = dto.WaterbodyId;
        entity.GuideId = dto.GuideId;
        await _db.SaveChangesAsync();

        return ServiceResult<FishingTripDto>.Ok((await GetByIdAsync(id))!);
    }

    public async Task<ServiceResult<bool>> DeleteAsync(int id)
    {
        var entity = await _db.FishingTrips.FindAsync(id);
        if (entity is null) return ServiceResult<bool>.NotFound($"Trip {id} not found");
        _db.FishingTrips.Remove(entity);
        await _db.SaveChangesAsync();
        return ServiceResult<bool>.Ok(true);
    }

    public async Task<ServiceResult<BookingDto>> BookAsync(int tripId, int anglerId)
    {
        var trip = await _db.FishingTrips
            .Include(t => t.Participants)
            .FirstOrDefaultAsync(t => t.Id == tripId);
        if (trip is null) return ServiceResult<BookingDto>.NotFound($"Trip {tripId} not found");

        var angler = await _db.Anglers.FindAsync(anglerId);
        if (angler is null) return ServiceResult<BookingDto>.NotFound($"Angler {anglerId} not found");

        if (trip.Status is TripStatus.Completed or TripStatus.Cancelled)
            return ServiceResult<BookingDto>.Conflict($"Cannot book a {trip.Status} trip");

        if (trip.Participants.Any(p => p.AnglerId == anglerId))
            return ServiceResult<BookingDto>.Conflict("Angler is already booked on this trip");

        if (trip.Participants.Count >= trip.MaxParticipants)
            return ServiceResult<BookingDto>.Conflict("Trip is fully booked");

        var booking = new TripParticipant
        {
            AnglerId = anglerId,
            FishingTripId = tripId,
            BookedAt = DateTime.UtcNow
        };
        _db.TripParticipants.Add(booking);
        await _db.SaveChangesAsync();

        return ServiceResult<BookingDto>.Ok(
            new BookingDto(anglerId, tripId, booking.BookedAt, false, null, angler.FullName));
    }

    public async Task<ServiceResult<bool>> CancelBookingAsync(int tripId, int anglerId)
    {
        var booking = await _db.TripParticipants.FindAsync(anglerId, tripId);
        if (booking is null) return ServiceResult<bool>.NotFound("Booking not found");
        _db.TripParticipants.Remove(booking);
        await _db.SaveChangesAsync();
        return ServiceResult<bool>.Ok(true);
    }

    public async Task<ServiceResult<FishingTripDto>> CompleteAsync(int tripId, CompleteTripDto data)
    {
        var trip = await _db.FishingTrips
            .Include(t => t.Participants)
            .FirstOrDefaultAsync(t => t.Id == tripId);
        if (trip is null) return ServiceResult<FishingTripDto>.NotFound($"Trip {tripId} not found");
        if (trip.Status == TripStatus.Completed)
            return ServiceResult<FishingTripDto>.Conflict("Trip is already completed");
        if (trip.Status == TripStatus.Cancelled)
            return ServiceResult<FishingTripDto>.Conflict("Cannot complete a cancelled trip");

        foreach (var p in trip.Participants)
        {
            p.Attended = data.CatchByAnglerId.ContainsKey(p.AnglerId);
            if (data.CatchByAnglerId.TryGetValue(p.AnglerId, out var weight))
                p.CatchWeightKg = weight;
        }
        trip.Status = TripStatus.Completed;
        await _db.SaveChangesAsync();

        return ServiceResult<FishingTripDto>.Ok((await GetByIdAsync(tripId))!);
    }

    public async Task<IEnumerable<BookingDto>> GetParticipantsAsync(int tripId)
    {
        var list = await _db.TripParticipants
            .Include(p => p.Angler)
            .Where(p => p.FishingTripId == tripId)
            .ToListAsync();
        return list.Select(p => new BookingDto(p.AnglerId, p.FishingTripId, p.BookedAt, p.Attended, p.CatchWeightKg, p.Angler!.FullName));
    }

    private static FishingTripDto ToDto(FishingTrip t) => new(
        t.Id, t.StartAt, t.EndAt, t.MaxParticipants,
        t.Participants?.Count ?? 0,
        t.MaxParticipants - (t.Participants?.Count ?? 0),
        t.PricePerPerson, t.Status,
        t.WaterbodyId, t.Waterbody?.Name ?? string.Empty,
        t.GuideId, t.Guide?.FullName ?? string.Empty);
}
