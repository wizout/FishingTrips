using FishingTrips.Api.Controllers;
using FishingTrips.Api.DTOs;
using FishingTrips.Api.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;

namespace FishingTrips.Tests.Controllers;

public class AnglersControllerTests
{
    [Fact]
    public async Task Create_ReturnsCreated_WithValidData()
    {
        var db = TestDbFactory.Create();
        var ctrl = new AnglersController(db);
        var r = await ctrl.Create(new AnglerCreateDto
        {
            FullName = "Test User", Email = "test@x.com", Level = ExperienceLevel.Beginner
        });
        r.Result.Should().BeOfType<CreatedAtActionResult>();
    }

    [Fact]
    public async Task Create_ReturnsConflict_ForDuplicateEmail()
    {
        var db = TestDbFactory.Create();
        db.Anglers.Add(new Angler { Id = 1, FullName = "X", Email = "dup@x.com" });
        await db.SaveChangesAsync();

        var ctrl = new AnglersController(db);
        var r = await ctrl.Create(new AnglerCreateDto { FullName = "Y", Email = "dup@x.com" });
        r.Result.Should().BeOfType<ConflictObjectResult>();
    }

    [Fact]
    public async Task Get_ReturnsNotFound_WhenMissing()
    {
        var db = TestDbFactory.Create();
        var ctrl = new AnglersController(db);
        (await ctrl.Get(999)).Result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetAll_ReturnsList()
    {
        var db = TestDbFactory.Create();
        db.Anglers.AddRange(
            new Angler { Id = 1, FullName = "A", Email = "a@x" },
            new Angler { Id = 2, FullName = "B", Email = "b@x" });
        await db.SaveChangesAsync();

        var ctrl = new AnglersController(db);
        var r = (await ctrl.GetAll()).Result as OkObjectResult;
        var list = r!.Value as IEnumerable<AnglerDto>;
        list.Should().HaveCount(2);
    }

    [Fact]
    public async Task Update_ReturnsNoContent_OnSuccess()
    {
        var db = TestDbFactory.Create();
        db.Anglers.Add(new Angler { Id = 1, FullName = "X", Email = "x@x" });
        await db.SaveChangesAsync();

        var ctrl = new AnglersController(db);
        var r = await ctrl.Update(1, new AnglerCreateDto { FullName = "Y", Email = "x@x" });
        r.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task Delete_ReturnsNotFound_WhenMissing()
    {
        var db = TestDbFactory.Create();
        var ctrl = new AnglersController(db);
        (await ctrl.Delete(999)).Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetTrips_ReturnsNotFound_ForMissingAngler()
    {
        var db = TestDbFactory.Create();
        var ctrl = new AnglersController(db);
        (await ctrl.GetTrips(999)).Result.Should().BeOfType<NotFoundResult>();
    }
}
