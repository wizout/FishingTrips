using FishingTrips.Api.Controllers;
using FishingTrips.Api.DTOs;
using FishingTrips.Api.Models;
using FishingTrips.Api.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace FishingTrips.Tests.Controllers;

public class FishingTripsControllerTests
{
    private static FishingTripDto SampleDto(int id = 1) =>
        new(id, DateTime.UtcNow, DateTime.UtcNow.AddHours(2), 4, 0, 4, 100m, TripStatus.Planned, 1, "W", 1, "G");

    [Fact]
    public async Task Get_ReturnsOk_WhenExists()
    {
        var mock = new Mock<IFishingTripService>();
        mock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(SampleDto());
        var ctrl = new FishingTripsController(mock.Object);
        (await ctrl.Get(1)).Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Get_ReturnsNotFound_WhenMissing()
    {
        var mock = new Mock<IFishingTripService>();
        mock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync((FishingTripDto?)null);
        var ctrl = new FishingTripsController(mock.Object);
        (await ctrl.Get(1)).Result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Create_ReturnsCreated_OnOk()
    {
        var mock = new Mock<IFishingTripService>();
        mock.Setup(s => s.CreateAsync(It.IsAny<FishingTripCreateDto>()))
            .ReturnsAsync(ServiceResult<FishingTripDto>.Ok(SampleDto(5)));
        var ctrl = new FishingTripsController(mock.Object);
        var r = await ctrl.Create(new FishingTripCreateDto());
        r.Result.Should().BeOfType<CreatedAtActionResult>();
    }

    [Fact]
    public async Task Create_ReturnsBadRequest_OnInvalid()
    {
        var mock = new Mock<IFishingTripService>();
        mock.Setup(s => s.CreateAsync(It.IsAny<FishingTripCreateDto>()))
            .ReturnsAsync(ServiceResult<FishingTripDto>.Invalid("bad"));
        var ctrl = new FishingTripsController(mock.Object);
        (await ctrl.Create(new FishingTripCreateDto())).Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Book_ReturnsConflict_WhenServiceConflict()
    {
        var mock = new Mock<IFishingTripService>();
        mock.Setup(s => s.BookAsync(1, 1))
            .ReturnsAsync(ServiceResult<BookingDto>.Conflict("full"));
        var ctrl = new FishingTripsController(mock.Object);
        var r = await ctrl.Book(1, new BookingCreateDto { AnglerId = 1 });
        r.Result.Should().BeOfType<ConflictObjectResult>();
    }

    [Fact]
    public async Task Book_ReturnsCreated_OnSuccess()
    {
        var mock = new Mock<IFishingTripService>();
        mock.Setup(s => s.BookAsync(1, 1))
            .ReturnsAsync(ServiceResult<BookingDto>.Ok(new BookingDto(1, 1, DateTime.UtcNow, false, null, "X")));
        var ctrl = new FishingTripsController(mock.Object);
        var r = await ctrl.Book(1, new BookingCreateDto { AnglerId = 1 });
        r.Result.Should().BeOfType<CreatedResult>();
    }

    [Fact]
    public async Task CancelBooking_ReturnsNoContent_OnSuccess()
    {
        var mock = new Mock<IFishingTripService>();
        mock.Setup(s => s.CancelBookingAsync(1, 1))
            .ReturnsAsync(ServiceResult<bool>.Ok(true));
        var ctrl = new FishingTripsController(mock.Object);
        (await ctrl.CancelBooking(1, 1)).Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task Delete_ReturnsNotFound_WhenMissing()
    {
        var mock = new Mock<IFishingTripService>();
        mock.Setup(s => s.DeleteAsync(1)).ReturnsAsync(ServiceResult<bool>.NotFound("no"));
        var ctrl = new FishingTripsController(mock.Object);
        (await ctrl.Delete(1)).Should().BeOfType<NotFoundResult>();
    }
}
