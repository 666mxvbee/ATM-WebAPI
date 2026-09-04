using Atm.Domain.Operations;
using Atm.Domain.Operations.Results;
using Atm.Domain.ValueObjects;

namespace Atm.Domain.Tests;

public sealed class BankTransactionTests
{
    private static readonly DateTimeOffset Now = new(2026, 9, 4, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void CommissionSnapshotUsesBankersRoundingAndStoresTheTariff()
    {
        var roundDown = new CommissionSnapshot(new Amount(1.0001m), 0.5m, 0m);
        var roundUp = new CommissionSnapshot(new Amount(1.0003m), 0.5m, 0m);

        Assert.Equal(0.5m, roundDown.Rate);
        Assert.Equal(0m, roundDown.FixedAmount.Value);
        Assert.Equal(0.5000m, roundDown.CommissionAmount.Value);
        Assert.Equal(0.5002m, roundUp.CommissionAmount.Value);
    }

    [Theory]
    [InlineData(-0.000001)]
    [InlineData(1.000001)]
    [InlineData(0.1234567)]
    public void CommissionSnapshotRejectsInvalidRate(decimal rate)
    {
        Assert.Throws<ArgumentException>(() => new CommissionSnapshot(new Amount(10m), rate, 0m));
    }

    [Fact]
    public void CommissionSnapshotRejectsInvalidFixedAmountAndPrecisionLoss()
    {
        Assert.Throws<ArgumentException>(() => new CommissionSnapshot(new Amount(10m), 0m, -1m));
        Assert.Throws<ArgumentException>(() => new CommissionSnapshot(new Amount(10m), 0m, 0.00001m));
        Assert.Throws<OverflowException>(() =>
            new CommissionSnapshot(new Amount(decimal.MaxValue), 1m, 0.0001m));
    }

    [Fact]
    public void CreateEnforcesAccountShapeRequestIdAndUtc()
    {
        var source = Guid.NewGuid();
        var target = Guid.NewGuid();
        var amount = new Amount(10m);
        var commission = new CommissionSnapshot(amount, 0m, 0m);

        Assert.IsType<BankTransactionCreationResult.Failure>(BankTransaction.Create(
            Guid.NewGuid(), Guid.Empty, BankTransactionType.Deposit, null, target, amount, commission, Now));
        Assert.IsType<BankTransactionCreationResult.Failure>(BankTransaction.Create(
            Guid.NewGuid(), Guid.NewGuid(), BankTransactionType.Deposit, source, target, amount, commission, Now));
        Assert.IsType<BankTransactionCreationResult.Failure>(BankTransaction.Create(
            Guid.NewGuid(), Guid.NewGuid(), BankTransactionType.Withdrawal, source, target, amount, commission, Now));
        Assert.IsType<BankTransactionCreationResult.Failure>(BankTransaction.Create(
            Guid.NewGuid(), Guid.NewGuid(), BankTransactionType.Transfer, source, source, amount, commission, Now));
        Assert.IsType<BankTransactionCreationResult.Failure>(BankTransaction.Create(
            Guid.NewGuid(), Guid.NewGuid(), BankTransactionType.Deposit, null, target, amount, commission,
            Now.ToOffset(TimeSpan.FromHours(3))));
    }

    [Fact]
    public void CreateRejectsMismatchedOrEconomicallyInvalidCommission()
    {
        var target = Guid.NewGuid();
        var originalAmount = new Amount(100m);
        var mismatchedAmount = new Amount(101m);
        var commission = new CommissionSnapshot(originalAmount, 0.1m, 0m);

        Assert.IsType<BankTransactionCreationResult.Failure>(BankTransaction.Create(
            Guid.NewGuid(), Guid.NewGuid(), BankTransactionType.Deposit,
            null, target, mismatchedAmount, commission, Now));

        var smallAmount = new Amount(1m);
        var fullCommission = new CommissionSnapshot(smallAmount, 0m, 1m);
        Assert.IsType<BankTransactionCreationResult.Failure>(BankTransaction.Create(
            Guid.NewGuid(), Guid.NewGuid(), BankTransactionType.Deposit,
            null, target, smallAmount, fullCommission, Now));
    }

    [Fact]
    public void HistoryEntryCalculatesDepositAndWithdrawalBalances()
    {
        var accountId = Guid.NewGuid();
        BankTransaction deposit = CreateTransaction(
            BankTransactionType.Deposit, null, accountId, 100m, 0m, 10m);
        AccountHistoryEntry depositEntry = Assert.IsType<AccountHistoryEntryCreationResult.Success>(
            AccountHistoryEntry.Create(Guid.NewGuid(), deposit, accountId, new Money(10m))).Entry;

        Assert.Equal(AccountHistoryEntryType.Deposit, depositEntry.EntryType);
        Assert.Equal(10m, depositEntry.BalanceBefore.Value);
        Assert.Equal(100m, depositEntry.BalanceAfter.Value);
        Assert.Equal(deposit.Id, depositEntry.TransactionId);
        Assert.Equal(deposit.CreatedAtUtc, depositEntry.CreatedAtUtc);

        BankTransaction withdrawal = CreateTransaction(
            BankTransactionType.Withdrawal, accountId, null, 20m, 0m, 2m);
        AccountHistoryEntry withdrawalEntry = Assert.IsType<AccountHistoryEntryCreationResult.Success>(
            AccountHistoryEntry.Create(Guid.NewGuid(), withdrawal, accountId, new Money(100m))).Entry;

        Assert.Equal(AccountHistoryEntryType.Withdrawal, withdrawalEntry.EntryType);
        Assert.Equal(78m, withdrawalEntry.BalanceAfter.Value);
    }

    [Fact]
    public void HistoryEntryProducesTwoSidesOfATransfer()
    {
        var source = Guid.NewGuid();
        var target = Guid.NewGuid();
        BankTransaction transaction = CreateTransaction(
            BankTransactionType.Transfer, source, target, 10m, 0m, 1m);

        AccountHistoryEntry debit = Assert.IsType<AccountHistoryEntryCreationResult.Success>(
            AccountHistoryEntry.Create(Guid.NewGuid(), transaction, source, new Money(100m))).Entry;
        AccountHistoryEntry credit = Assert.IsType<AccountHistoryEntryCreationResult.Success>(
            AccountHistoryEntry.Create(Guid.NewGuid(), transaction, target, new Money(50m))).Entry;

        Assert.Equal(AccountHistoryEntryType.TransferDebit, debit.EntryType);
        Assert.Equal(89m, debit.BalanceAfter.Value);
        Assert.Equal(AccountHistoryEntryType.TransferCredit, credit.EntryType);
        Assert.Equal(60m, credit.BalanceAfter.Value);
        Assert.Equal(debit.TransactionId, credit.TransactionId);
    }

    [Fact]
    public void HistoryEntryFailureDoesNotChangeTheProvidedBalance()
    {
        var source = Guid.NewGuid();
        BankTransaction transaction = CreateTransaction(
            BankTransactionType.Withdrawal, source, null, 10m, 0m, 1m);
        var balance = new Money(10m);

        Assert.IsType<AccountHistoryEntryCreationResult.Failure>(
            AccountHistoryEntry.Create(Guid.NewGuid(), transaction, source, balance));
        Assert.Equal(10m, balance.Value);
        Assert.IsType<AccountHistoryEntryCreationResult.Failure>(
            AccountHistoryEntry.Create(Guid.NewGuid(), transaction, Guid.NewGuid(), new Money(100m)));
    }

    private static BankTransaction CreateTransaction(
        BankTransactionType type,
        Guid? source,
        Guid? target,
        decimal amountValue,
        decimal rate,
        decimal fixedAmount)
    {
        var amount = new Amount(amountValue);
        var commission = new CommissionSnapshot(amount, rate, fixedAmount);
        return Assert.IsType<BankTransactionCreationResult.Success>(BankTransaction.Create(
            Guid.NewGuid(), Guid.NewGuid(), type, source, target, amount, commission, Now)).Transaction;
    }
}
