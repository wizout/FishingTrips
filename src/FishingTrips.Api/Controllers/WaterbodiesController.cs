using FishingTrips.Api.Data;
using FishingTrips.Api.DTOs;
using FishingTrips.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FishingTrips.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WaterbodiesController : ControllerBase
{
    private readonly AppDbContext _db;
    public WaterbodiesController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<WaterbodyDto>>> GetAll() =>
        Ok(await _db.Waterbodies.AsNoTracking()
            .Select(w => new WaterbodyDto(w.Id, w.Name, w.Type, w.Location, w.AreaHa, w.FishSpecies))
            .ToListAsync());

    [HttpGet("{id:int}")]
    public async Task<ActionResult<WaterbodyDto>> Get(int id)
    {
        var w = await _db.Waterbodies.FindAsync(id);
        return w is null ? NotFound() : Ok(new WaterbodyDto(w.Id, w.Name, w.Type, w.Location, w.AreaHa, w.FishSpecies));
    }

    [HttpGet("{id:int}/trips")]
    public async Task<ActionResult<IEnumerable<FishingTripDto>>> GetTrips(int id)
    {
        if (!await _db.Waterbodies.AnyAsync(x => x.Id == id)) return NotFound();
        var list = await _db.FishingTrips
            .Where(t => t.WaterbodyId == id)
            .Include(t => t.Waterbody)
            .Include(t => t.Guide)
            .Include(t => t.Participants)
            .ToListAsync();
        return Ok(list.Select(t => new FishingTripDto(
            t.Id, t.StartAt, t.EndAt, t.MaxParticipants,
            t.Participants.Count, t.MaxParticipants - t.Participants.Count,
            t.PricePerPerson, t.Status,
            t.WaterbodyId, t.Waterbody?.Name ?? "",
            t.GuideId, t.Guide?.FullName ?? "")));
    }

    [HttpPost]
    public async Task<ActionResult<WaterbodyDto>> Create(WaterbodyCreateDto dto)
    {
        var entity = new Waterbody
        {
            Name = dto.Name, Type = dto.Type, Location = dto.Location,
            AreaHa = dto.AreaHa, FishSpecies = dto.FishSpecies
        };
        _db.Waterbodies.Add(entity);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = entity.Id },
            new WaterbodyDto(entity.Id, entity.Name, entity.Type, entity.Location, entity.AreaHa, entity.FishSpecies));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, WaterbodyCreateDto dto)
    {
        var entity = await _db.Waterbodies.FindAsync(id);
        if (entity is null) return NotFound();
        entity.Name = dto.Name; entity.Type = dto.Type; entity.Location = dto.Location;
        entity.AreaHa = dto.AreaHa; entity.FishSpecies = dto.FishSpecies;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _db.Waterbodies.FindAsync(id);
        if (entity is null) return NotFound();
        if (await _db.FishingTrips.AnyAsync(t => t.WaterbodyId == id))
            return Conflict(new ProblemDetails { Title = "Waterbody has trips; cannot delete", Status = 409 });
        _db.Waterbodies.Remove(entity);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
