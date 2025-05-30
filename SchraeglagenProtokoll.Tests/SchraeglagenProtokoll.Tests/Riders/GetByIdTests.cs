using Alba;
using SchraeglagenProtokoll.Api.Riders;

namespace SchraeglagenProtokoll.Tests.Riders;

public class GetByIdTests(WebAppFixture fixture) : WebAppTestBase(fixture)
{
    [Test]
    public async Task when_getting_a_rider_by_id_then_it_is_returned()
    {
        // Arrange
        var riderId = await StartStream(EventFaker.RiderRegistered());

        // Act
        var result = Scenario(x =>
        {
            x.Get.Url($"/rider/{riderId}");
            x.StatusCodeShouldBeOk();
        });

        // Assert
        var rider = result.Result.ReadAsJson<Rider>();
        await Verify(rider);
    }
}
