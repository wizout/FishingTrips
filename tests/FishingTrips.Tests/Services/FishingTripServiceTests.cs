using FishingTrips.Api.DTOs;
using FishingTrips.Api.Models;
using FishingTrips.Api.Services;
using FluentAssertions;

namespace FishingTrips.Tests.Services;

public class FishingTripServiceTests
{
    private static async Task<(FishingTripService svc, Api.Data.AppDbContext db)> SetupAsync()
    {
        var db = TestDbFactory.Create();
        await TestDbFactory.SeedBasicAsync(db);
        return (new FishingTripService(db), db);
    }

    [Fact]
    public async Task GetAll_ReturnsAllTrips()
    {
        var (svc, _) = await SetupAsync();
        var result = await svc.GetAllAsync(null, null, null, null);
        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetAll_FiltersByWaterbody()
    {
        var (svc, _) = await SetupAsync();
        var hit = await svc.GetAllAsync(30, null, null, null);
        var miss = await svc.GetAllAsync(999, null, null, null);
        hit.Should().HaveCount(1);
        miss.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAll_FiltersByDateRange()
    {
        var (svc, _) = await SetupAsync();
        var inRange = await svc.GetAllAsync(null, null, new DateTime(2026,5,1), new DateTime(2026,7,1));
        var outRange = await svc.GetAllAsync(null, null, new DateTime(2027,1,1), null);
        inRange.Should().HaveCount(1);
        outRange.Should().BeEmpty();
    }

    [Fact]
    public async Task GetById_ReturnsNullForMissing()
    {
        var (svc, _) = await SetupAsync();
        (await svc.GetByIdAsync(9999)).Should().BeNull();
    }

    [Fact]
    public async Task Create_Succeeds_WithValidData()
    {
        var (svc, _) = await SetupAsync();
        var r = await svc.CreateAsync(new FishingTripCreateDto
        {
            StartAt = DateTime.UtcNow.AddDays(10),
            EndAt = DateTime.UtcNow.AddDays(10).AddHours(4),
            MaxParticipants = 3, PricePerPerson = 800m,
            WaterbodyId = 30, GuideId = 20
        });
        r.IsSuccess.Should().BeTrue();
        r.Value!.WaterbodyId.Should().Be(30);
    }

    [Fact]
    public async Task Create_Fails_WhenEndBeforeStart()
    {
        var (svc, _) = await SetupAsync();
        var r = await svc.CreateAsync(new FishingTripCreateDto
        {
            StartAt = DateTime.UtcNow.AddDays(1),
            EndAt = DateTime.UtcNow,
            MaxParticipants = 2, PricePerPerson = 100m, WaterbodyId = 30, GuideId = 20
        });
        r.Status.Should().Be(ResultStatus.Invalid);
    }

    [Fact]
    public async Task Create_Fails_WhenWaterbodyNotFound()
    {
        var (svc, _) = await SetupAsync();
        var r = await svc.CreateAsync(new FishingTripCreateDto
        {
            StartAt = DateTime.UtcNow, EndAt = DateTime.UtcNow.AddHours(2),
            MaxParticipants = 2, PricePerPerson = 100m, WaterbodyId = 999, GuideId = 20
        });
        r.Status.Should().Be(ResultStatus.NotFound);
    }

    [Fact]
    public async Task Create_Fails_WhenGuideNotFound()
    {
        var (svc, _) = await SetupAsync();
        var r = await svc.CreateAsync(new FishingTripCreateDto
        {
            StartAt = DateTime.UtcNow, EndAt = DateTime.UtcNow.AddHours(2),
            MaxParticipants = 2, PricePerPerson = 100m, WaterbodyId = 30, GuideId = 999
        });
        r.Status.Should().Be(ResultStatus.NotFound);
    }

    [Fact]
    public async Task Delete_RemovesTrip()
    {
        var (svc, db) = await SetupAsync();
        var r = await svc.DeleteAsync(40);
        r.IsSuccess.Should().BeTrue();
        db.FishingTrips.Should().BeEmpty();
    }

    [Fact]
    public async Task Delete_NotFound_WhenMissing()
    {
        var (svc, _) = await SetupAsync();
        var r = await svc.DeleteAsync(9999);
        r.Status.Should().Be(ResultStatus.NotFound);
    }

    [Fact]
    public async Task Update_Fails_WhenReducingMaxBelowExistingBookings()
    {
        var (svc, db) = await SetupAsync();
        db.TripParticipants.Add(new TripParticipant { AnglerId = 10, FishingTripId = 40 });
        db.TripParticipants.Add(new TripParticipant { AnglerId = 11, FishingTripId = 40 });
        await db.SaveChangesAsync();

        var r = await svc.UpdateAsync(40, new FishingTripCreateDto
        {
            StartAt = DateTime.UtcNow, EndAt = DateTime.UtcNow.AddHours(2),
            MaxParticipants = 1, PricePerPerson = 100m, WaterbodyId = 30, GuideId = 20
        });
        r.Status.Should().Be(ResultStatus.Conflict);
    }

    [Fact]
    public async Task Book_Succeeds_OnPlannedTripWithFreeSlots()
    {
        var (svc, _) = await SetupAsync();
        var r = await svc.BookAsync(40, 10);
        r.IsSuccess.Should().BeTrue();
        r.Value!.AnglerId.Should().Be(10);
    }

    [Fact]
    public async Task Book_Fails_WhenAlreadyBooked()
    {
        var (svc, _) = await SetupAsync();
        await svc.BookAsync(40, 10);
        var r = await svc.BookAsync(40, 10);
        r.Status.Should().Be(ResultStatus.Conflict);
        r.Error.Should().Contain("already");
    }

    [Fact]
    public async Task Book_Fails_WhenTripIsFull()
    {
        var (svc, _) = await SetupAsync();
        (await svc.BookAsync(40, 10)).IsSuccess.Should().BeTrue();
        (await svc.BookAsync(40, 11)).IsSuccess.Should().BeTrue();
        var r = await svc.BookAsync(40, 12);
        r.Status.Should().Be(ResultStatus.Conflict);
        r.Error.Should().Contain("fully");
    }

    [Fact]
    public async Task Book_Fails_WhenTripCompleted()
    {
        var (svc, db) = await SetupAsync();
        var trip = await db.FishingTrips.FindAsync(40);
        trip!.Status = TripStatus.Completed;
        await db.SaveChangesAsync();

        var r = await svc.BookAsync(40, 10);
        r.Status.Should().Be(ResultStatus.Conflict);
    }

    [Fact]
    public async Task Book_Fails_WhenAnglerMissing()
    {
        var (svc, _) = await SetupAsync();
        var r = await svc.BookAsync(40, 9999);
        r.Status.Should().Be(ResultStatus.NotFound);
    }

    [Fact]
    public async Task Book_Fails_WhenTripMissing()
    {
        var (svc, _) = await SetupAsync();
        var r = await svc.BookAsync(9999, 10);
        r.Status.Should().Be(ResultStatus.NotFound);
    }

    [Fact]
    public async Task FreeSlots_DecreasesAfterBooking()
    {
        var (svc, _) = await SetupAsync();
        var before = await svc.GetByIdAsync(40);
        before!.FreeSlots.Should().Be(2);
        await svc.BookAsync(40, 10);
        var after = await svc.GetByIdAsync(40);
        after!.FreeSlots.Should().Be(1);
        after.BookedCount.Should().Be(1);
    }

    [Fact]
    public async Task CancelBooking_RemovesParticipant()
    {
        var (svc, _) = await SetupAsync();
        await svc.BookAsync(40, 10);
        var r = await svc.CancelBookingAsync(40, 10);
        r.IsSuccess.Should().BeTrue();
        var trip = await svc.GetByIdAsync(40);
        trip!.BookedCount.Should().Be(0);
    }

    [Fact]
    public async Task CancelBooking_NotFound_WhenNoBooking()
    {
        var (svc, _) = await SetupAsync();
        var r = await svc.CancelBookingAsync(40, 10);
        r.Status.Should().Be(ResultStatus.NotFound);
    }

    [Fact]
    public async Task Complete_MarksTripCompletedAndStoresCatch()
    {
        var (svc, _) = await SetupAsync();
        await svc.BookAsync(40, 10);
        await svc.BookAsync(40, 11);

        var data = new CompleteTripDto { CatchByAnglerId = new() { [10] = 2.5m, [11] = 1.0m } };
        var r = await svc.CompleteAsync(40, data);
        r.IsSuccess.Should().BeTrue();
        r.Value!.Status.Should().Be(TripStatus.Completed);

        var participants = await svc.GetParticipantsAsync(40);
        participants.Should().AllSatisfy(p => p.Attended.Should().BeTrue());
        participants.Sum(p => p.CatchWeightKg).Should().Be(3.5m);
    }

    [Fact]
    public async Task Complete_Fails_WhenAlreadyCompleted()
    {
        var (svc, _) = await SetupAsync();
        await svc.CompleteAsync(40, new CompleteTripDto());
        var r = await svc.CompleteAsync(40, new CompleteTripDto());
        r.Status.Should().Be(ResultStatus.Conflict);
    }

    [Fact]
    public async Task Complete_Fails_WhenCancelled()
    {
        var (svc, db) = await SetupAsync();
        var t = await db.FishingTrips.FindAsync(40);
        t!.Status = TripStatus.Cancelled;
        await db.SaveChangesAsync();
        var r = await svc.CompleteAsync(40, new CompleteTripDto());
        r.Status.Should().Be(ResultStatus.Conflict);
    }

    [Fact]
    public async Task GetParticipants_ReturnsBookings()
    {
        var (svc, _) = await SetupAsync();
        await svc.BookAsync(40, 10);
        await svc.BookAsync(40, 11);
        var ps = await svc.GetParticipantsAsync(40);
        ps.Should().HaveCount(2);
    }

    [Fact]
    public async Task Update_Succeeds_AndChangesFields()
    {
        var (svc, _) = await SetupAsync();
        var r = await svc.UpdateAsync(40, new FishingTripCreateDto
        {
            StartAt = new DateTime(2026,7,1,10,0,0,DateTimeKind.Utc),
            EndAt = new DateTime(2026,7,1,15,0,0,DateTimeKind.Utc),
            MaxParticipants = 5, PricePerPerson = 999m,
            WaterbodyId = 30, GuideId = 20
        });
        r.IsSuccess.Should().BeTrue();
        r.Value!.MaxParticipants.Should().Be(5);
        r.Value.PricePerPerson.Should().Be(999m);
    }

    [Fact]
    public async Task Update_NotFound_WhenMissing()
    {
        var (svc, _) = await SetupAsync();
        var r = await svc.UpdateAsync(9999, new FishingTripCreateDto
        {
            StartAt = DateTime.UtcNow, EndAt = DateTime.UtcNow.AddHours(1),
            MaxParticipants = 1, PricePerPerson = 1m, WaterbodyId = 30, GuideId = 20
        });
        r.Status.Should().Be(ResultStatus.NotFound);
    }
}
