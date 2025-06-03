using System.Diagnostics;
using Alba;
using SchraeglagenProtokoll.Api.Riders;
using Shouldly;

namespace SchraeglagenProtokoll.Tests.Riders;

public class GetByIdTests_WithAbsurdeEventCounts(WebAppFixture fixture) : WebAppTestBase(fixture)
{
    private static readonly int AbsurdeEventCount = 100_000;

    [Test, Skip("Execution takes long, run them when you like to")]
    public async Task when_getting_a_rider_by_id_by_aggregation_then_it_is_returned()
    {
        // Arrange
        var riderId = Guid.NewGuid();
        var renameEvents = Enumerable
            .Range(0, AbsurdeEventCount)
            .Select(_ => FakeEvent.RiderRenamed(riderId))
            .ToList();
        await StartStream(riderId, [FakeEvent.RiderRegistered(riderId), .. renameEvents]);

        // Act
        var stopwatch = Stopwatch.StartNew();
        var result = await Scenario(x =>
        {
            x.Get.Url($"/rider/{riderId}/by-aggregation");
            x.StatusCodeShouldBeOk();
        });
        stopwatch.Stop();

        // Assert
        Context.Current.OutputWriter.WriteLine(
            $"Time to get rider: {stopwatch.ElapsedMilliseconds} ms"
        );
        var rider = result.ReadAsJson<Rider>();
        rider.FullName.ShouldBe(renameEvents.Last().FullName);
    }

    [Test, Skip("Execution takes long, run them when you like to")]
    public async Task when_getting_a_rider_by_id_from_projection_then_it_is_returned()
    {
        // Arrange
        var riderId = Guid.NewGuid();
        var renameEvents = Enumerable
            .Range(0, AbsurdeEventCount)
            .Select(_ => FakeEvent.RiderRenamed(riderId))
            .ToList();
        await StartStream(riderId, [FakeEvent.RiderRegistered(riderId), .. renameEvents]);

        // Act
        var stopwatch = Stopwatch.StartNew();
        var result = await Scenario(x =>
        {
            x.Get.Url($"/rider/{riderId}/from-projection");
            x.StatusCodeShouldBeOk();
        });
        stopwatch.Stop();

        // Assert
        Context.Current.OutputWriter.WriteLine(
            $"Time to get rider: {stopwatch.ElapsedMilliseconds} ms"
        );
        var rider = result.ReadAsJson<Rider>();
        rider.FullName.ShouldBe(renameEvents.Last().FullName);
    }

    [Test, Skip("Execution takes long, run them when you like to")]
    public async Task when_getting_a_rider_by_id_from_streamed_projection_then_it_is_returned()
    {
        // Arrange
        var riderId = Guid.NewGuid();
        var renameEvents = Enumerable
            .Range(0, AbsurdeEventCount)
            .Select(_ => FakeEvent.RiderRenamed(riderId))
            .ToList();
        await StartStream(riderId, [FakeEvent.RiderRegistered(riderId), .. renameEvents]);

        // Act
        var stopwatch = Stopwatch.StartNew();
        var result = await Scenario(x =>
        {
            x.Get.Url($"/rider/{riderId}/from-streamed-projection");
            x.StatusCodeShouldBeOk();
        });
        stopwatch.Stop();

        // Assert
        Context.Current.OutputWriter.WriteLine(
            $"Time to get rider: {stopwatch.ElapsedMilliseconds} ms"
        );
        var rider = result.ReadAsJson<Rider>();
        rider.FullName.ShouldBe(renameEvents.Last().FullName);
    }
}
