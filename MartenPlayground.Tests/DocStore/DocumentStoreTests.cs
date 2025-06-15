using ImTools;
using Marten;
using Marten.Services;
using MartenPlayground.Tests.Setup;
using Shouldly;

namespace MartenPlayground.Tests.DocStore;

public record UserRecord(Guid Id, string FirstName, string LastName);

public class UserClass(Guid id, string firstName, string lastName)
{
    public Guid Id { get; set; } = id;
    public string FirstName { get; set; } = firstName;
    public string LastName { get; set; } = lastName;
}

public class DocumentStoreTests : TestBase
{
    public static readonly string DsDemo1 = nameof(DsDemo1);
    public static readonly string DsDemo2 = nameof(DsDemo2);

    [Before(Class)]
    public static async Task CleanupSchema()
    {
        await ResetAllData(DsDemo1);
        await ResetAllData(DsDemo2);
    }

    [Test]
    public async Task T1_create_documentstore_save_and_load_document()
    {
        // Create Marten DocumentStore from a connection string
        var store = DocumentStore.For(options =>
        {
            options.Connection(ConnectionString);
            options.DatabaseSchemaName = DsDemo1;
        });

        // Create a document
        await using var writeSession = store.LightweightSession(); // Suits most needs
        var johnDoe = new UserClass(Guid.NewGuid(), "John", "Doe");
        writeSession.Store(johnDoe);
        await writeSession.SaveChangesAsync();

        // Load document
        await using var readSession = store.QuerySession(); // For Read-Only operations
        var loadedJohnDoe = await readSession.LoadAsync<UserClass>(johnDoe.Id);
        loadedJohnDoe
            .ShouldNotBeNull()
            .ShouldSatisfyAllConditions(
                u => u.FirstName.ShouldBe("John"),
                u => u.LastName.ShouldBe("Doe")
            );

        // Change with Dirty-Tracking and save it
        await using var dirtyTrackedSession = store.DirtyTrackedSession( // Dirty check (with cached JSON)
            SessionOptions.ForConnectionString(ConnectionString)
        );
        var trackedJohnDoe = await dirtyTrackedSession.LoadAsync<UserClass>(johnDoe.Id);
        // dirtyUser = dirtyUser with { FirstName = "Jane" }; // does not work with records -> copy
        trackedJohnDoe!.FirstName = "Jane";
        await dirtyTrackedSession.SaveChangesAsync();

        // Load document again
        var loadedJaneDoe = await readSession.LoadAsync<UserClass>(trackedJohnDoe.Id);
        loadedJaneDoe.ShouldNotBeNull().FirstName.ShouldBe("Jane");
    }

    [Test]
    public async Task T2_create_five_test_users()
    {
        // Arrange
        UserRecord[] users =
        [
            new(Guid.NewGuid(), "Michael", "Smith"),
            new(Guid.NewGuid(), "Sarah", "Johnson"),
            new(Guid.NewGuid(), "David", "Williams"),
            new(Guid.NewGuid(), "Emily", "Brown"),
            new(Guid.NewGuid(), "James", "Anderson"),
        ];

        // Act
        await using var session = Session(DsDemo2);
        session.Store(users);
        await session.SaveChangesAsync();

        // Assert
        session.Query<UserRecord>().Count().ShouldBe(5);

        AddToBag("Users", users);
    }

    [Test]
    [DependsOn(nameof(T2_create_five_test_users))]
    public async Task T3_load_five_test_users()
    {
        // Arrange
        var arrangedUsers = GetFromBag<UserRecord[]>(nameof(T2_create_five_test_users), "Users");
        var arrangedUserIds = arrangedUsers.Select(u => u.Id).ToList();

        // Act
        await using var session = Session(DsDemo2);
        var loadedUsers = await session.LoadManyAsync<UserRecord>(arrangedUserIds);

        // Assert
        loadedUsers.Count.ShouldBe(5);
    }

    [Test]
    [DependsOn(nameof(T3_load_five_test_users))]
    public async Task T4_when_sahra_johnson_married_she_is_upserted_to_sarah_anderson()
    {
        // Arrange
        var arrangedUsers = GetFromBag<UserRecord[]>(nameof(T2_create_five_test_users), "Users");
        var sarahJohnson = arrangedUsers.FindFirst(x =>
            x.FirstName == "Sarah" && x.LastName == "Johnson"
        );

        // Act
        await using var session = Session(DsDemo2);
        session.Store(sarahJohnson with { LastName = "Anderson" });
        await session.SaveChangesAsync();

        // Assert
        var sarahAnderson = await session.LoadAsync<UserRecord>(sarahJohnson.Id);
        sarahAnderson
            .ShouldNotBeNull()
            .ShouldBe(new UserRecord(sarahJohnson.Id, "Sarah", "Anderson"));
    }

    [Test]
    [DependsOn(nameof(T4_when_sahra_johnson_married_she_is_upserted_to_sarah_anderson))]
    public async Task T5_querying_andersons_returns_sarah_and_james()
    {
        // Act
        await using var session = Session(DsDemo2);
        var andersons = await session
            .Query<UserRecord>()
            .Where(x => x.LastName == "Anderson")
            .ToListAsync();
        await session.SaveChangesAsync();

        // Assert
        andersons.Count.ShouldBe(2);
        andersons.ShouldContain(x => x.FirstName == "Sarah");
        andersons.ShouldContain(x => x.FirstName == "James");
    }

    [Test]
    [DependsOn(nameof(T5_querying_andersons_returns_sarah_and_james))]
    public async Task T6_delete_michael_smith()
    {
        // Arrange
        var arrangedUsers = GetFromBag<UserRecord[]>(nameof(T2_create_five_test_users), "Users");
        var michaelSmith = arrangedUsers.FindFirst(x =>
            x.FirstName == "Michael" && x.LastName == "Smith"
        );

        // Act
        await using var session = Session(DsDemo2);
        session.Delete<UserRecord>(michaelSmith.Id);
        await session.SaveChangesAsync();

        // Assert
        var loadedUsers = await session.LoadManyAsync<UserRecord>(arrangedUsers.Select(u => u.Id));
        loadedUsers.Count.ShouldBe(4);
        loadedUsers.ShouldNotContain(michaelSmith);
    }
}
