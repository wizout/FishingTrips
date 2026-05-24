using FishingTrips.Api.DTOs;
using FishingTrips.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace FishingTrips.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FishingTripsController : ControllerBase
{
    private readonly IFishingTripService _svc;
    public FishingTripsController(IFishingTripService svc) => _svc = svc;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<FishingTripDto>>> GetAll(
        [FromQuery] int? waterbodyId, [FromQuery] int? guideId,
        [FromQuery] DateTime? from, [FromQuery] DateTime? to) =>
        Ok(await _svc.GetAllAsync(waterbodyId, guideId, from, to));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<FishingTripDto>> Get(int id)
    {
        var dto = await _svc.GetByIdAsync(id);
        return dto is null ? NotFound() : Ok(dto);
    }

    [HttpGet("{id:int}/participants")]
    public async Task<ActionResult<IEnumerable<BookingDto>>> GetParticipants(int id) =>
        Ok(await _svc.GetParticipantsAsync(id));

    [HttpPost]
    public async Task<ActionResult<FishingTripDto>> Create(FishingTripCreateDto dto)
    {
        var r = await _svc.CreateAsync(dto);
        return Map(r, created: true);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, FishingTripCreateDto dto)
    {
        var r = await _svc.UpdateAsync(id, dto);
        return r.Status switch
        {
            ResultStatus.Ok => NoContent(),
            ResultStatus.NotFound => NotFound(new ProblemDetails { Title = r.Error, Status = 404 }),
            ResultStatus.Conflict => Conflict(new ProblemDetails { Title = r.Error, Status = 409 }),
            _ => BadRequest(new ProblemDetails { Title = r.Error, Status = 400 })
        };
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var r = await _svc.DeleteAsync(id);
        return r.Status == ResultStatus.NotFound ? NotFound() : NoContent();
    }

    [HttpPost("{id:int}/book")]
    public async Task<ActionResult<BookingDto>> Book(int id, BookingCreateDto dto)
    {
        var r = await _svc.BookAsync(id, dto.AnglerId);
        return r.Status switch
        {
            ResultStatus.Ok => Created($"/api/fishingtrips/{id}/participants", r.Value),
            ResultStatus.NotFound => NotFound(new ProblemDetails { Title = r.Error, Status = 404 }),
            ResultStatus.Conflict => Conflict(new ProblemDetails { Title = r.Error, Status = 409 }),
            _ => BadRequest(new ProblemDetails { Title = r.Error, Status = 400 })
        };
    }

    [HttpDelete("{id:int}/book/{anglerId:int}")]
    public async Task<IActionResult> CancelBooking(int id, int anglerId)
    {
        var r = await _svc.CancelBookingAsync(id, anglerId);
        return r.Status == ResultStatus.NotFound ? NotFound() : NoContent();
    }

    [HttpPost("{id:int}/complete")]
    public async Task<ActionResult<FishingTripDto>> Complete(int id, CompleteTripDto data)
    {
        var r = await _svc.CompleteAsync(id, data ?? new CompleteTripDto());
        return Map(r, created: false);
    }

    private ActionResult<FishingTripDto> Map(ServiceResult<FishingTripDto> r, bool created) => r.Status switch
    {
        ResultStatus.Ok => created
            ? CreatedAtAction(nameof(Get), new { id = r.Value!.Id }, r.Value)
            : Ok(r.Value),
        ResultStatus.NotFound => NotFound(new ProblemDetails { Title = r.Error, Status = 404 }),
        ResultStatus.Conflict => Conflict(new ProblemDetails { Title = r.Error, Status = 409 }),
        _ => BadRequest(new ProblemDetails { Title = r.Error, Status = 400 })
    };
}
