using Alba;
using SchraeglagenProtokoll.Api.Riders;

namespace SchraeglagenProtokoll.Tests.Riders;

public class GetByIdTests(WebAppFixture fixture) : WebAppTestBase(fixture)
{
    [Test]
    public async Task when_getting_a_rider_by_id_by_aggregation_then_it_is_returned()
    {
        // Arrange
        var riderId = Guid.NewGuid();
        await StartStream(riderId, FakeEvent.RiderRegistered(riderId));

        // Act
        var result = await Scenario(x =>
        {
            x.Get.Url($"/rider/{riderId}/by-aggregation");
            x.StatusCodeShouldBeOk();
        });

        // Assert
        var rider = result.ReadAsJson<Rider>();
        await Verify(rider);
    }

    [Test]
    public async Task when_getting_a_rider_by_id_from_projection_then_it_is_returned()
    {
        // Arrange
        var riderId = Guid.NewGuid();
        await StartStream(riderId, FakeEvent.RiderRegistered(riderId));

        // Act
        var result = await Scenario(x =>
        {
            x.Get.Url($"/rider/{riderId}/from-projection");
            x.StatusCodeShouldBeOk();
        });

        // Assert
        var rider = result.ReadAsJson<Rider>();
        await Verify(rider);
    }

    [Test]
    public async Task when_getting_a_rider_by_id_from_streamed_projection_then_it_is_returned()
    {
        // Arrange
        var riderId = Guid.NewGuid();
        await StartStream(riderId, FakeEvent.RiderRegistered(riderId));

        // Act
        var result = await Scenario(x =>
        {
            x.Get.Url($"/rider/{riderId}/from-streamed-projection");
            x.StatusCodeShouldBeOk();
        });

        // Assert
        var rider = result.ReadAsJson<Rider>();
        await Verify(rider);
    }
}
