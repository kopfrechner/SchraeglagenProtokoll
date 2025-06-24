using ImTools;
using Marten;
using MartenPlayground.Tests.Model;
using MartenPlayground.Tests.Setup;
using Shouldly;

namespace MartenPlayground.Tests.DocStore;

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
        await using var lightweightSession = store.LightweightSession(); // Suits most needs
        var johnDoeAccount = new BankAccount("John Doe", Money.From(1000, Currency.USD));
        lightweightSession.Store(johnDoeAccount);
        await lightweightSession.SaveChangesAsync();

        // Load document
        await using var querySession = store.QuerySession(); // For Read-Only operations
        var loadedAccount = await querySession.LoadAsync<BankAccount>(johnDoeAccount.Id);
        loadedAccount
            .ShouldNotBeNull()
            .ShouldSatisfyAllConditions(
                a => a.Owner.ShouldBe("John Doe"),
                a => a.Balance.Amount.ShouldBe(1000),
                a => a.Balance.Currency.ShouldBe(Currency.USD)
            );
    }

    [Test]
    public async Task T2_create_five_test_accounts()
    {
        // Arrange
        BankAccount[] accounts =
        [
            new("Michael Smith", Money.From(5000, Currency.EUR)),
            new("Sarah Johnson", Money.From(3200, Currency.USD)),
            new("David Williams", Money.From(1800, Currency.CHF)),
            new("Emily Brown", Money.From(2500, Currency.EUR)),
            new("James Anderson", Money.From(4100, Currency.USD)),
        ];

        // Act
        await using var session = Session(DsDemo2);
        session.Store(accounts);
        await session.SaveChangesAsync();

        // Assert
        session.Query<BankAccount>().Count().ShouldBe(5);

        AddToBag("Accounts", accounts);
    }

    [Test]
    [DependsOn(nameof(T2_create_five_test_accounts))]
    public async Task T3_load_five_test_accounts()
    {
        // Arrange
        var arrangedAccounts = GetFromBag<BankAccount[]>(
            nameof(T2_create_five_test_accounts),
            "Accounts"
        );
        var arrangedAccountIds = arrangedAccounts.Select(a => a.Id).ToList();

        // Act
        await using var session = Session(DsDemo2);
        var loadedAccounts = await session.LoadManyAsync<BankAccount>(arrangedAccountIds);

        // Assert
        loadedAccounts.Count.ShouldBe(5);
    }

    [Test]
    [DependsOn(nameof(T3_load_five_test_accounts))]
    public async Task T4_when_sarah_johnson_account_balance_increases()
    {
        // Arrange
        var arrangedAccounts = GetFromBag<BankAccount[]>(
            nameof(T2_create_five_test_accounts),
            "Accounts"
        );
        var sarahJohnsonAccount = arrangedAccounts.FindFirst(x => x.Owner == "Sarah Johnson");

        // Act
        await using var session = Session(DsDemo2);
        session.Store(sarahJohnsonAccount with { Balance = Money.From(4500, Currency.USD) });
        await session.SaveChangesAsync();

        // Assert
        var updatedAccount = await session.LoadAsync<BankAccount>(sarahJohnsonAccount.Id);
        updatedAccount.ShouldNotBeNull();
        updatedAccount.Owner.ShouldBe("Sarah Johnson");
        updatedAccount.Balance.Amount.ShouldBe(4500);
        updatedAccount.Balance.Currency.ShouldBe(Currency.USD);
    }

    [Test]
    [DependsOn(nameof(T4_when_sarah_johnson_account_balance_increases))]
    public async Task T5_querying_usd_accounts_returns_sarah_and_james()
    {
        // Act
        await using var session = Session(DsDemo2);
        // https://martendb.io/documents/querying/linq/
        var usdAccounts = await session
            .Query<BankAccount>()
            .Where(x => x.Balance.Currency == Currency.USD)
            .ToListAsync();
        await session.SaveChangesAsync();

        // Assert
        usdAccounts.Count.ShouldBe(2);
        usdAccounts.ShouldContain(x => x.Owner == "Sarah Johnson");
        usdAccounts.ShouldContain(x => x.Owner == "James Anderson");
    }

    [Test]
    [DependsOn(nameof(T5_querying_usd_accounts_returns_sarah_and_james))]
    public async Task T6_delete_michael_smith_account()
    {
        // Arrange
        var arrangedAccounts = GetFromBag<BankAccount[]>(
            nameof(T2_create_five_test_accounts),
            "Accounts"
        );
        var michaelSmithAccount = arrangedAccounts.FindFirst(x => x.Owner == "Michael Smith");

        // Act
        await using var session = Session(DsDemo2);
        session.Delete<BankAccount>(michaelSmithAccount.Id);
        await session.SaveChangesAsync();

        // Assert
        var loadedAccounts = await session.LoadManyAsync<BankAccount>(
            arrangedAccounts.Select(a => a.Id)
        );
        loadedAccounts.Count.ShouldBe(4);
        loadedAccounts.ShouldNotContain(michaelSmithAccount);
    }
}
