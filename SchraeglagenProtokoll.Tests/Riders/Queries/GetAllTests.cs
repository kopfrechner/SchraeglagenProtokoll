using SchraeglagenProtokoll.Api.Responses;
using SchraeglagenProtokoll.Api.Riders;

namespace SchraeglagenProtokoll.Tests.Riders.Queries;

/// <remarks>
/// PagedList is somehow not retrieved...
/// Use List instead of PagedList to get the tests to pass.
/// </remarks>>
public class GetAllTests(WebAppFixture fixture) : WebAppTestBase(fixture)
{
    [Test]
    public async Task when_getting_all_riders_without_pagination_then_default_page_is_returned()
    {
        // Arrange
        await CreateTestRiders(5);

        // Act
        var result = await Scenario(x =>
        {
            x.Get.Url("/rider");
            x.StatusCodeShouldBe(200);
        });

        // Assert
        var pagedList = result.ReadAsJson<PagedListResponse<Rider>>();
        await Verify(pagedList);
    }

    [Test]
    public async Task when_getting_all_riders_with_pagination_then_correct_page_is_returned()
    {
        // Arrange
        await CreateTestRiders(15);

        // Act
        var result = await Scenario(x =>
        {
            x.Get.Url("/rider?page-number=2&page-size=5");
            x.StatusCodeShouldBe(200);
        });

        // Assert
        var pagedList = result.ReadAsJson<PagedListResponse<Rider>>();
        await Verify(pagedList);
    }

    [Test]
    public async Task when_getting_all_riders_with_large_page_size_then_all_riders_are_returned()
    {
        // Arrange
        await CreateTestRiders(8);

        // Act
        var result = await Scenario(x =>
        {
            x.Get.Url("/rider?page-size=20");
            x.StatusCodeShouldBe(200);
        });

        // Assert
        var pagedList = result.ReadAsJson<PagedListResponse<Rider>>();
        await Verify(pagedList);
    }

    [Test]
    public async Task when_getting_riders_beyond_available_pages_then_empty_page_is_returned()
    {
        // Arrange
        await CreateTestRiders(5);

        // Act
        var result = await Scenario(x =>
        {
            x.Get.Url("/rider?page-number=10&page-size=5");
            x.StatusCodeShouldBe(200);
        });

        // Assert
        var pagedList = result.ReadAsJson<PagedListResponse<Rider>>();
        await Verify(pagedList);
    }

    [Test]
    public async Task when_searching_riders_by_full_name_then_matching_riders_are_returned()
    {
        // Arrange
        await CreateSpecificTestRiders();

        // Act
        var result = await Scenario(x =>
        {
            x.Get.Url("/rider?search-term=Smith");
            x.StatusCodeShouldBe(200);
        });

        // Assert
        var pagedList = result.ReadAsJson<PagedListResponse<Rider>>();
        await Verify(pagedList);
    }

    [Test]
    public async Task when_searching_riders_by_nerd_alias_then_matching_riders_are_returned()
    {
        // Arrange
        await CreateSpecificTestRiders();

        // Act
        var result = await Scenario(x =>
        {
            x.Get.Url("/rider?search-term=Shadow");
            x.StatusCodeShouldBe(200);
        });

        // Assert
        var pagedList = result.ReadAsJson<PagedListResponse<Rider>>();
        await Verify(pagedList);
    }

    [Test]
    public async Task when_searching_riders_case_insensitive_then_matching_riders_are_returned()
    {
        // Arrange
        await CreateSpecificTestRiders();

        // Act
        var result = await Scenario(x =>
        {
            x.Get.Url("/rider?search-term=THUNDER");
            x.StatusCodeShouldBe(200);
        });

        // Assert
        var pagedList = result.ReadAsJson<PagedListResponse<Rider>>();
        await Verify(pagedList);
    }

    [Test]
    public async Task when_searching_riders_with_partial_match_then_matching_riders_are_returned()
    {
        // Arrange
        await CreateSpecificTestRiders();

        // Act
        var result = await Scenario(x =>
        {
            x.Get.Url("/rider?search-term=John");
            x.StatusCodeShouldBe(200);
        });

        // Assert
        var pagedList = result.ReadAsJson<PagedListResponse<Rider>>();
        await Verify(pagedList);
    }

    [Test]
    public async Task when_searching_riders_with_no_matches_then_empty_page_is_returned()
    {
        // Arrange
        await CreateSpecificTestRiders();

        // Act
        var result = await Scenario(x =>
        {
            x.Get.Url("/rider?search-term=NonExistentName");
            x.StatusCodeShouldBe(200);
        });

        // Assert
        var pagedList = result.ReadAsJson<PagedListResponse<Rider>>();
        await Verify(pagedList);
    }

    [Test]
    public async Task when_searching_riders_with_pagination_then_correct_filtered_page_is_returned()
    {
        // Arrange
        await CreateManyRidersWithSimilarNames();

        // Act
        var result = await Scenario(x =>
        {
            x.Get.Url("/rider?search-term=Test&page-number=2&page-size=3");
            x.StatusCodeShouldBe(200);
        });

        // Assert
        var pagedList = result.ReadAsJson<PagedListResponse<Rider>>();
        await Verify(pagedList);
    }

    [Test]
    public async Task when_getting_riders_with_empty_search_term_then_all_riders_are_returned()
    {
        // Arrange
        await CreateTestRiders(5);

        // Act
        var result = await Scenario(x =>
        {
            x.Get.Url("/rider?search-term=");
            x.StatusCodeShouldBe(200);
        });

        // Assert
        var pagedList = result.ReadAsJson<PagedListResponse<Rider>>();
        await Verify(pagedList);
    }

    private async Task CreateTestRiders(int count)
    {
        for (int i = 1; i <= count; i++)
        {
            var riderId = Guid.NewGuid();
            var riderRegistered = FakeEvent.RiderRegistered(riderId);
            await StartStream(riderId, riderRegistered);
        }
    }

    private async Task CreateSpecificTestRiders()
    {
        var riders = new[]
        {
            FakeEvent.RiderRegistered(
                Guid.NewGuid(),
                email: "john.smith@test.com",
                fullName: "John Smith",
                roadName: "Thunder"
            ),
            FakeEvent.RiderRegistered(
                Guid.NewGuid(),
                email: "jane.doe@test.com",
                fullName: "Jane Doe",
                roadName: "Shadow"
            ),
            FakeEvent.RiderRegistered(
                Guid.NewGuid(),
                email: "bob.johnson@test.com",
                fullName: "Bob Johnson",
                roadName: "Lightning"
            ),
            FakeEvent.RiderRegistered(
                Guid.NewGuid(),
                email: "alice.smith@test.com",
                fullName: "Alice Smith",
                roadName: "Storm"
            ),
            FakeEvent.RiderRegistered(
                Guid.NewGuid(),
                email: "charlie.brown@test.com",
                fullName: "Charlie Brown",
                roadName: "Rider"
            ),
        };

        foreach (var rider in riders)
        {
            await StartStream(rider.RiderId, rider);
        }
    }

    private async Task CreateManyRidersWithSimilarNames()
    {
        for (int i = 1; i <= 10; i++)
        {
            var riderId = Guid.NewGuid();
            var rider = FakeEvent.RiderRegistered(
                riderId,
                email: $"test{i}@test.com",
                fullName: $"Test Rider {i}",
                roadName: $"TestAlias{i}"
            );
            await StartStream(riderId, rider);
        }
    }
}
