using FishingTrips.Api.Data;
using FishingTrips.Api.DTOs;
using FishingTrips.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FishingTrips.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GuidesController : ControllerBase
{
    private readonly AppDbContext _db;
    public GuidesController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<GuideDto>>> GetAll() =>
        Ok(await _db.Guides.AsNoTracking()
            .Select(g => new GuideDto(g.Id, g.FullName, g.LicenseNumber, g.YearsExperience, g.Bio))
            .ToListAsync());

    [HttpGet("{id:int}")]
    public async Task<ActionResult<GuideDto>> Get(int id)
    {
        var g = await _db.Guides.FindAsync(id);
        return g is null ? NotFound() : Ok(new GuideDto(g.Id, g.FullName, g.LicenseNumber, g.YearsExperience, g.Bio));
    }

    [HttpPost]
    public async Task<ActionResult<GuideDto>> Create(GuideCreateDto dto)
    {
        if (await _db.Guides.AnyAsync(x => x.LicenseNumber == dto.LicenseNumber))
            return Conflict(new ProblemDetails { Title = "LicenseNumber already exists", Status = 409 });

        var entity = new Guide
        {
            FullName = dto.FullName, LicenseNumber = dto.LicenseNumber,
            YearsExperience = dto.YearsExperience, Bio = dto.Bio
        };
        _db.Guides.Add(entity);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = entity.Id },
            new GuideDto(entity.Id, entity.FullName, entity.LicenseNumber, entity.YearsExperience, entity.Bio));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, GuideCreateDto dto)
    {
        var entity = await _db.Guides.FindAsync(id);
        if (entity is null) return NotFound();
        if (entity.LicenseNumber != dto.LicenseNumber && await _db.Guides.AnyAsync(x => x.LicenseNumber == dto.LicenseNumber))
            return Conflict(new ProblemDetails { Title = "LicenseNumber already exists", Status = 409 });

        entity.FullName = dto.FullName;
        entity.LicenseNumber = dto.LicenseNumber;
        entity.YearsExperience = dto.YearsExperience;
        entity.Bio = dto.Bio;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _db.Guides.FindAsync(id);
        if (entity is null) return NotFound();
        if (await _db.FishingTrips.AnyAsync(t => t.GuideId == id))
            return Conflict(new ProblemDetails { Title = "Guide has trips; cannot delete", Status = 409 });
        _db.Guides.Remove(entity);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
