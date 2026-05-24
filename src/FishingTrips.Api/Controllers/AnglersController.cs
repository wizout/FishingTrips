using FishingTrips.Api.Data;
using FishingTrips.Api.DTOs;
using FishingTrips.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FishingTrips.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AnglersController : ControllerBase
{
    private readonly AppDbContext _db;
    public AnglersController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AnglerDto>>> GetAll() =>
        Ok(await _db.Anglers.AsNoTracking()
            .Select(a => new AnglerDto(a.Id, a.FullName, a.Email, a.Phone, a.RegisteredAt, a.Level))
            .ToListAsync());

    [HttpGet("{id:int}")]
    public async Task<ActionResult<AnglerDto>> Get(int id)
    {
        var a = await _db.Anglers.FindAsync(id);
        return a is null ? NotFound() : Ok(new AnglerDto(a.Id, a.FullName, a.Email, a.Phone, a.RegisteredAt, a.Level));
    }

    [HttpGet("{id:int}/trips")]
    public async Task<ActionResult<IEnumerable<FishingTripDto>>> GetTrips(int id)
    {
        if (!await _db.Anglers.AnyAsync(x => x.Id == id)) return NotFound();
        var list = await _db.TripParticipants
            .Where(p => p.AnglerId == id)
            .Include(p => p.FishingTrip!).ThenInclude(t => t.Waterbody)
            .Include(p => p.FishingTrip!).ThenInclude(t => t.Guide)
            .Include(p => p.FishingTrip!).ThenInclude(t => t.Participants)
            .Select(p => p.FishingTrip!)
            .ToListAsync();
        return Ok(list.Select(t => new FishingTripDto(
            t.Id, t.StartAt, t.EndAt, t.MaxParticipants,
            t.Participants.Count, t.MaxParticipants - t.Participants.Count,
            t.PricePerPerson, t.Status,
            t.WaterbodyId, t.Waterbody?.Name ?? "",
            t.GuideId, t.Guide?.FullName ?? "")));
    }

    [HttpPost]
    public async Task<ActionResult<AnglerDto>> Create(AnglerCreateDto dto)
    {
        if (await _db.Anglers.AnyAsync(a => a.Email == dto.Email))
            return Conflict(new ProblemDetails { Title = "Email already exists", Status = 409 });

        var entity = new Angler
        {
            FullName = dto.FullName, Email = dto.Email, Phone = dto.Phone,
            Level = dto.Level, RegisteredAt = DateTime.UtcNow
        };
        _db.Anglers.Add(entity);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = entity.Id },
            new AnglerDto(entity.Id, entity.FullName, entity.Email, entity.Phone, entity.RegisteredAt, entity.Level));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, AnglerCreateDto dto)
    {
        var entity = await _db.Anglers.FindAsync(id);
        if (entity is null) return NotFound();
        if (entity.Email != dto.Email && await _db.Anglers.AnyAsync(a => a.Email == dto.Email))
            return Conflict(new ProblemDetails { Title = "Email already exists", Status = 409 });

        entity.FullName = dto.FullName;
        entity.Email = dto.Email;
        entity.Phone = dto.Phone;
        entity.Level = dto.Level;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _db.Anglers.FindAsync(id);
        if (entity is null) return NotFound();
        _db.Anglers.Remove(entity);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
