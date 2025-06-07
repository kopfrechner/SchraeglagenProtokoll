using ImTools;
using Marten;
using MartenPlayground.Tests.Setup;
using Shouldly;

namespace MartenPlayground.Tests.DocStore;

public record User(Guid Id, string FirstName, string LastName);

public class DocumentStoreTests(PostgresContainerFixture fixture) : DocumentStoreTestBase(fixture)
{
    private const string DocumentStoreScenario01 = nameof(DocumentStoreScenario01);
    private const string DocumentStoreScenario02 = nameof(DocumentStoreScenario02);
    
    [Test]
    public async Task O1_create_documentstore_then_save_document_then_load_document()
    {
        // Create Marten DocumentStore from a connection string
        var store = DocumentStore.For(options =>
        {
            options.Connection(ConnectionString);
            options.DatabaseSchemaName = DocumentStoreScenario01;
        });
        await using var session = store.LightweightSession();

        // Create a document
        var user = new User(Guid.NewGuid(), "John", "Doe");
        session.Store(user);
        await session.SaveChangesAsync();

        // Load document
        var loadedUser = await session.LoadAsync<User>(user.Id);
        loadedUser
            .ShouldNotBeNull()
            .ShouldSatisfyAllConditions(
                u => u.FirstName.ShouldBe("John"),
                u => u.LastName.ShouldBe("Doe")
            );
    }

    [Test]
    public async Task O2_when_create_test_users_should_succeed()
    {
        // Arrange
        User[] users =
        [
            new(Guid.NewGuid(), "Michael", "Smith"),
            new(Guid.NewGuid(), "Sarah", "Johnson"),
            new(Guid.NewGuid(), "David", "Williams"),
            new(Guid.NewGuid(), "Emily", "Brown"),
            new(Guid.NewGuid(), "James", "Anderson"),
        ];

        // Act
        await using var session = Session(DocumentStoreScenario02);
        session.Store(users);
        await session.SaveChangesAsync();

        // Assert
        session.Query<User>().Count().ShouldBe(5);

        AddToBag("Users", users);
    }

    [Test]
    [DependsOn(nameof(O2_when_create_test_users_should_succeed))]
    public async Task O3_when_load_test_users_then_should_be_5()
    {
        // Arrange
        var arrangedUsers = GetFromBag<User[]>(
            nameof(O2_when_create_test_users_should_succeed),
            "Users"
        );
        var arrangedUserIds = arrangedUsers.Select(u => u.Id).ToList();

        // Act
        await using var session = Session(DocumentStoreScenario02);
        var loadedUsers = await session.LoadManyAsync<User>(arrangedUserIds);

        // Assert
        loadedUsers.Count.ShouldBe(5);
    }

    [Test]
    [DependsOn(nameof(O3_when_load_test_users_then_should_be_5))]
    public async Task O4_when_sahra_johnson_married_then_she_is_upserted_to_sarah_anderson()
    {
        // Arrange
        var arrangedUsers = GetFromBag<User[]>(
            nameof(O2_when_create_test_users_should_succeed),
            "Users"
        );
        var sarahJohnson = arrangedUsers.FindFirst(x =>
            x.FirstName == "Sarah" && x.LastName == "Johnson"
        );

        // Act
        await using var session = Session(DocumentStoreScenario02);
        session.Store(sarahJohnson with { LastName = "Anderson" });
        await session.SaveChangesAsync();

        // Assert
        var sarahAnderson = await session.LoadAsync<User>(sarahJohnson.Id);
        sarahAnderson.ShouldNotBeNull().ShouldBe(new User(sarahJohnson.Id, "Sarah", "Anderson"));
    }

    
    [Test]
    [DependsOn(nameof(O4_when_sahra_johnson_married_then_she_is_upserted_to_sarah_anderson))]
    public async Task O5_when_querying_for_the_andersons_then_sarah_and_james_are_returned()
    {
        // Act
        await using var session = Session(DocumentStoreScenario02);
        var andersons = await session.Query<User>().Where(x => x.LastName == "Anderson").ToListAsync();
        await session.SaveChangesAsync();

        // Assert
        andersons.Count.ShouldBe(2);
        andersons.ShouldContain(x => x.FirstName == "Sarah");
        andersons.ShouldContain(x => x.FirstName == "James");
    }
    
    [Test]
    [DependsOn(nameof(O5_when_querying_for_the_andersons_then_sarah_and_james_are_returned))]
    public async Task O6_when_deleted_michael_smith_then_michael_smith_is_not_loaded()
    {
        // Arrange
        var arrangedUsers = GetFromBag<User[]>(
            nameof(O2_when_create_test_users_should_succeed),
            "Users"
        );
        var michaelSmith = arrangedUsers.FindFirst(x =>
            x.FirstName == "Michael" && x.LastName == "Smith"
        );

        // Act
        await using var session = Session(DocumentStoreScenario02);
        session.Delete<User>(michaelSmith.Id);
        await session.SaveChangesAsync();

        // Assert
        var loadedUsers = await session.LoadManyAsync<User>(arrangedUsers.Select(u => u.Id));
        loadedUsers.Count.ShouldBe(4);
        loadedUsers.ShouldNotContain(michaelSmith);
    }
}
