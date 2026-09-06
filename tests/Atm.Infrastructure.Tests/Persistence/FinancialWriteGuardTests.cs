using Atm.Domain.Accounts;
using Atm.Domain.Accounts.Results;
using Atm.Domain.Operations;
using Atm.Domain.Operations.Results;
using Atm.Domain.ValueObjects;

namespace Atm.Infrastructure.Tests.Persistence;

public sealed class FinancialWriteGuardTests
{
    private static readonly DateTimeOffset Now = new(2026, 9, 6, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void DeletingAnAccountFailsBeforeDatabaseAccess()
    {
        using Infrastructure.Persistence.AtmDbContext context = ModelTestContext.Create();
        Account account = CreateAccount();
        context.Attach(account);
        context.Remove(account);
        InvalidOperationException error = Assert.Throws<InvalidOperationException>(() => context.SaveChanges());
        Assert.Equal("Physical account deletion is not supported.", error.Message);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task UpdatingOrDeletingHistoryFailsBeforeDatabaseAccess(bool delete)
    {
        using Infrastructure.Persistence.AtmDbContext context = ModelTestContext.Create();
        BankTransaction transaction = CreateTransaction();
        AccountHistoryEntry history = Assert.IsType<AccountHistoryEntryCreationResult.Success>(
            AccountHistoryEntry.Create(Guid.NewGuid(), transaction, transaction.TargetAccountId!.Value, new Money(10m))).Entry;
        context.Attach(history);
        if (delete)
            context.Remove(history);
        else
            context.Entry(history).Property(entry => entry.BalanceAfter).CurrentValue = new Money(20m);

        InvalidOperationException error = await Assert.ThrowsAsync<InvalidOperationException>(() => context.SaveChangesAsync());
        Assert.Equal("Persisted financial records are immutable.", error.Message);
    }

    [Fact]
    public void ChangingTheCommissionSnapshotFailsBeforeDatabaseAccess()
    {
        using Infrastructure.Persistence.AtmDbContext context = ModelTestContext.Create();
        BankTransaction transaction = CreateTransaction();
        context.Attach(transaction);
        context.Entry(transaction).Reference(value => value.Commission).TargetEntry!
            .Property(nameof(CommissionSnapshot.Rate)).CurrentValue = 0.5m;
        InvalidOperationException error = Assert.Throws<InvalidOperationException>(() => context.SaveChanges());
        Assert.Equal("Persisted financial records are immutable.", error.Message);
    }

    [Fact]
    public void ChangingBalanceTracksTheVersionAndConvertedValue()
    {
        using Infrastructure.Persistence.AtmDbContext context = ModelTestContext.Create();
        Account account = CreateAccount();
        context.Attach(account);
        account.Deposit(new Amount(2m), Now.AddMinutes(1));
        context.ChangeTracker.DetectChanges();
        Assert.True(context.Entry(account).Property(value => value.Balance).IsModified);
        Assert.Equal(0, context.Entry(account).Property(value => value.Version).OriginalValue);
        Assert.Equal(1, context.Entry(account).Property(value => value.Version).CurrentValue);
    }

    private static Account CreateAccount()
    {
        return Assert.IsType<AccountCreationResult.Success>(Account.Create(
            Guid.NewGuid(), new AccountNumber("guard"), Guid.NewGuid(), new PinHash("test-hash"),
            new Money(10m), Now)).Account;
    }

    private static BankTransaction CreateTransaction()
    {
        var amount = new Amount(1m);
        return Assert.IsType<BankTransactionCreationResult.Success>(BankTransaction.Create(
            Guid.NewGuid(), Guid.NewGuid(), BankTransactionType.Deposit, null, Guid.NewGuid(),
            amount, new CommissionSnapshot(amount, 0m, 0m), Now)).Transaction;
    }
}
