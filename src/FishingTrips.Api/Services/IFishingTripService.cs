using FishingTrips.Api.DTOs;

namespace FishingTrips.Api.Services;

public interface IFishingTripService
{
    Task<IEnumerable<FishingTripDto>> GetAllAsync(int? waterbodyId, int? guideId, DateTime? from, DateTime? to);
    Task<FishingTripDto?> GetByIdAsync(int id);
    Task<ServiceResult<FishingTripDto>> CreateAsync(FishingTripCreateDto dto);
    Task<ServiceResult<FishingTripDto>> UpdateAsync(int id, FishingTripCreateDto dto);
    Task<ServiceResult<bool>> DeleteAsync(int id);

    Task<ServiceResult<BookingDto>> BookAsync(int tripId, int anglerId);
    Task<ServiceResult<bool>> CancelBookingAsync(int tripId, int anglerId);
    Task<ServiceResult<FishingTripDto>> CompleteAsync(int tripId, CompleteTripDto data);
    Task<IEnumerable<BookingDto>> GetParticipantsAsync(int tripId);
}
