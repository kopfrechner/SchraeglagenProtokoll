using ImTools;
using Marten;
using MartenPlayground.Tests.Setup;
using Shouldly;

namespace MartenPlayground.Tests.DocStore;

public record User(Guid Id, string FirstName, string LastName);

public class DocumentStoreTests(PostgresContainerFixture fixture) : TestBase(fixture)
{
    private const string DSDemo1 = nameof(DSDemo1);
    private const string DSDemo2 = nameof(DSDemo2);

    [Test]
    public async Task T1_create_documentstore_then_save_document_then_load_document()
    {
        // Create Marten DocumentStore from a connection string
        var store = DocumentStore.For(options =>
        {
            options.Connection(ConnectionString);
            options.DatabaseSchemaName = DSDemo1;
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
    public async Task T2_when_create_test_users_should_succeed()
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
        await using var session = Session(DSDemo2);
        session.Store(users);
        await session.SaveChangesAsync();

        // Assert
        session.Query<User>().Count().ShouldBe(5);

        AddToBag("Users", users);
    }

    [Test]
    [DependsOn(nameof(T2_when_create_test_users_should_succeed))]
    public async Task T3_when_load_test_users_then_should_be_5()
    {
        // Arrange
        var arrangedUsers = GetFromBag<User[]>(
            nameof(T2_when_create_test_users_should_succeed),
            "Users"
        );
        var arrangedUserIds = arrangedUsers.Select(u => u.Id).ToList();

        // Act
        await using var session = Session(DSDemo2);
        var loadedUsers = await session.LoadManyAsync<User>(arrangedUserIds);

        // Assert
        loadedUsers.Count.ShouldBe(5);
    }

    [Test]
    [DependsOn(nameof(T3_when_load_test_users_then_should_be_5))]
    public async Task T4_when_sahra_johnson_married_then_she_is_upserted_to_sarah_anderson()
    {
        // Arrange
        var arrangedUsers = GetFromBag<User[]>(
            nameof(T2_when_create_test_users_should_succeed),
            "Users"
        );
        var sarahJohnson = arrangedUsers.FindFirst(x =>
            x.FirstName == "Sarah" && x.LastName == "Johnson"
        );

        // Act
        await using var session = Session(DSDemo2);
        session.Store(sarahJohnson with { LastName = "Anderson" });
        await session.SaveChangesAsync();

        // Assert
        var sarahAnderson = await session.LoadAsync<User>(sarahJohnson.Id);
        sarahAnderson.ShouldNotBeNull().ShouldBe(new User(sarahJohnson.Id, "Sarah", "Anderson"));
    }

    [Test]
    [DependsOn(nameof(T4_when_sahra_johnson_married_then_she_is_upserted_to_sarah_anderson))]
    public async Task T5_when_querying_for_the_andersons_then_sarah_and_james_are_returned()
    {
        // Act
        await using var session = Session(DSDemo2);
        var andersons = await session
            .Query<User>()
            .Where(x => x.LastName == "Anderson")
            .ToListAsync();
        await session.SaveChangesAsync();

        // Assert
        andersons.Count.ShouldBe(2);
        andersons.ShouldContain(x => x.FirstName == "Sarah");
        andersons.ShouldContain(x => x.FirstName == "James");
    }

    [Test]
    [DependsOn(nameof(T5_when_querying_for_the_andersons_then_sarah_and_james_are_returned))]
    public async Task T6_when_deleted_michael_smith_then_michael_smith_is_not_loaded()
    {
        // Arrange
        var arrangedUsers = GetFromBag<User[]>(
            nameof(T2_when_create_test_users_should_succeed),
            "Users"
        );
        var michaelSmith = arrangedUsers.FindFirst(x =>
            x.FirstName == "Michael" && x.LastName == "Smith"
        );

        // Act
        await using var session = Session(DSDemo2);
        session.Delete<User>(michaelSmith.Id);
        await session.SaveChangesAsync();

        // Assert
        var loadedUsers = await session.LoadManyAsync<User>(arrangedUsers.Select(u => u.Id));
        loadedUsers.Count.ShouldBe(4);
        loadedUsers.ShouldNotContain(michaelSmith);
    }
}
