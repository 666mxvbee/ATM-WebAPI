using Atm.Domain.Accounts;
using Atm.Domain.Operations;
using Atm.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Atm.Infrastructure.Tests.Persistence;

public sealed class AtmModelTests
{
    [Fact]
    public void ModelContainsExactlyTheRequiredTables()
    {
        using Infrastructure.Persistence.AtmDbContext context = ModelTestContext.Create();
        IModel model = context.GetService<IDesignTimeModel>().Model;
        string[] tables = model.GetRelationalModel().Tables.Select(table => table.Name).Order().ToArray();
        Assert.Equal(new[]
        {
            "account_history_entries", "accounts", "bank_transactions", "friendships", "sessions", "users",
        }, tables);
        Assert.Null(model.FindEntityType(typeof(OperationRecord)));
        Assert.All(model.GetRelationalModel().Tables, table => Assert.Equal("public", table.Schema));
        Assert.All(model.GetEntityTypes().Where(entity => !entity.IsOwned()), entity =>
        {
            Assert.NotNull(entity.FindPrimaryKey());
            Assert.All(entity.FindPrimaryKey()!.Properties, property =>
                Assert.Equal(ValueGenerated.Never, property.ValueGenerated));
        });
    }

    [Fact]
    public void SchemaScriptContainsConstraintsAndNoMoneyPrecisionCeiling()
    {
        using Infrastructure.Persistence.AtmDbContext context = ModelTestContext.Create();
        string sql = context.Database.GenerateCreateScript();
        Assert.Contains("CREATE TABLE public.users", sql, StringComparison.Ordinal);
        Assert.Contains("user_id < friend_user_id", sql, StringComparison.Ordinal);
        Assert.Contains("ON DELETE RESTRICT", sql, StringComparison.Ordinal);
        Assert.Contains("ON DELETE CASCADE", sql, StringComparison.Ordinal);
        Assert.Contains("balance numeric NOT NULL", sql, StringComparison.Ordinal);
        Assert.Contains("commission_rate numeric(9,6) NOT NULL", sql, StringComparison.Ordinal);
        Assert.DoesNotContain("numeric(19,4)", sql, StringComparison.Ordinal);
        Assert.Contains("trunc(balance, 4)", sql, StringComparison.Ordinal);
        Assert.Contains("normalized_login", sql, StringComparison.Ordinal);
        Assert.Contains("normalized_number", sql, StringComparison.Ordinal);
    }

    [Fact]
    public void VersionIndexesAndDeletionRulesMatchTheSpecification()
    {
        using Infrastructure.Persistence.AtmDbContext context = ModelTestContext.Create();
        IModel model = context.GetService<IDesignTimeModel>().Model;
        IEntityType account = model.FindEntityType(typeof(Account))!;
        Assert.True(account.FindProperty(nameof(Account.Version))!.IsConcurrencyToken);
        Assert.All(model.GetEntityTypes().Where(entity => !entity.IsOwned())
            .SelectMany(entity => entity.GetForeignKeys())
            .Where(key => key.DeclaringEntityType.GetTableName() != "friendships"),
            key => Assert.Equal(DeleteBehavior.Restrict, key.DeleteBehavior));

        IIndex timeline = model.FindEntityType(typeof(AccountHistoryEntry))!.GetIndexes()
            .Single(index => index.GetDatabaseName() == "ix_account_history_entries_account_timeline");
        Assert.Equal(new[] { false, true, true }, timeline.IsDescending);
        Assert.Equal(new[] { "AccountId", "CreatedAtUtc", "Id" }, timeline.Properties.Select(property => property.Name));
        Assert.True(model.FindEntityType(typeof(BankTransaction))!.GetIndexes()
            .Single(index => index.Properties.Single().Name == "RequestId").IsUnique);
        Assert.Equal(2, model.FindEntityType(typeof(AccountHistoryEntry))!.GetKeys().Count());
    }

    [Fact]
    public void ValueConversionsRoundTripAndCompareByValue()
    {
        using Infrastructure.Persistence.AtmDbContext context = ModelTestContext.Create();
        IProperty balance = context.Model.FindEntityType(typeof(Account))!.FindProperty(nameof(Account.Balance))!;
        var original = new Money(1_000_000_000_000_001.0001m);
        Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter converter = balance.GetTypeMapping().Converter!;
        object? databaseValue = converter.ConvertToProvider(original);
        Money restored = Assert.IsType<Money>(converter.ConvertFromProvider(databaseValue));
        Assert.Equal(original.Value, restored.Value);
        Assert.True(balance.GetValueComparer()!.Equals(original, restored));
        Assert.False(balance.GetValueComparer()!.Equals(original, new Money(1m)));

        IProperty amount = context.Model.FindEntityType(typeof(BankTransaction))!.FindProperty("Amount")!;
        Amount restoredAmount = Assert.IsType<Amount>(
            amount.GetTypeMapping().Converter!.ConvertFromProvider(0.0001m));
        Assert.Equal(0.0001m, restoredAmount.Value);
    }

    [Fact]
    public void NormalizedLookupsAndHistoryPaginationTranslateToSql()
    {
        using Infrastructure.Persistence.AtmDbContext context = ModelTestContext.Create();
        string users = context.Users.Where(user => user.Login.NormalizedValue == "USER").ToQueryString();
        string accounts = context.Accounts.Where(account => account.Number.NormalizedValue == "ACCOUNT").ToQueryString();
        string history = context.AccountHistoryEntries.Where(entry => entry.AccountId == Guid.Empty)
            .OrderByDescending(entry => entry.CreatedAtUtc).ThenByDescending(entry => entry.Id)
            .Take(20).ToQueryString();
        Assert.Contains("normalized_login", users, StringComparison.Ordinal);
        Assert.Contains("normalized_number", accounts, StringComparison.Ordinal);
        Assert.Contains("DESC", history, StringComparison.Ordinal);
        Assert.Contains("LIMIT", history, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("users", "normalized_login", "ux_users_normalized_login")]
    [InlineData("accounts", "normalized_number", "ux_accounts_normalized_number")]
    [InlineData("bank_transactions", "request_id", "ux_bank_transactions_request_id")]
    public void BusinessKeysHaveRequiredUniqueIndexes(string tableName, string columnName, string indexName)
    {
        using Infrastructure.Persistence.AtmDbContext context = ModelTestContext.Create();
        ITable table = context.GetService<IDesignTimeModel>().Model.GetRelationalModel().FindTable(tableName, "public")!;
        Assert.False(table.FindColumn(columnName)!.IsNullable);
        ITableIndex index = table.Indexes.Single(value => value.Name == indexName);
        Assert.True(index.IsUnique);
        Assert.Equal(columnName, Assert.Single(index.Columns).Name);
    }
}
