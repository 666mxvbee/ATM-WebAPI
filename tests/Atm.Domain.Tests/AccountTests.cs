using Atm.Domain.Accounts;
using Atm.Domain.Accounts.Results;
using Atm.Domain.ValueObjects;

namespace Atm.Domain.Tests;

public class AccountTests
{
    [Fact]
    public void CreateRequiresAnOwnerAndUtcTime()
    {
        var number = new AccountNumber("test");
        var hash = new PinHash("test-only-hash");
        DateTimeOffset now = DomainTestData.UtcNow;

        Assert.IsType<AccountCreationResult.Failure>(Account.Create(
            Guid.NewGuid(), number, Guid.Empty, hash, new Money(0m), now));
        Assert.IsType<AccountCreationResult.Failure>(Account.Create(
            Guid.NewGuid(), number, Guid.NewGuid(), hash, new Money(0m), now.ToOffset(TimeSpan.FromHours(3))));
    }

    [Fact]
    public void SuccessfulChangesAdvanceTheVersionAndTimestamp()
    {
        Account account = DomainTestData.CreateAccount(new AccountNumber("test"), 100m);
        DateTimeOffset now = account.CreatedAtUtc.AddMinutes(1);

        Assert.IsType<DepositResult.Success>(account.Deposit(new Amount(10m), now));
        Assert.Equal(1, account.Version);
        Assert.Equal(now, account.UpdatedAtUtc);
        Assert.IsType<WithdrawResult.Success>(account.Withdraw(new Amount(20m), now.AddMinutes(1)));
        Assert.Equal(2, account.Version);
        Assert.Equal(90m, account.Balance.Value);
    }

    [Fact]
    public void FailedChangesDoNotMutateTheAccount()
    {
        Account account = DomainTestData.CreateAccount(new AccountNumber("test"), 100m);
        DateTimeOffset now = account.CreatedAtUtc;

        Assert.IsType<WithdrawResult.Failure>(account.Withdraw(new Amount(101m), now));
        Assert.IsType<DepositResult.Failure>(account.Deposit(new Amount(1m), now.AddTicks(-1)));
        Assert.Equal(100m, account.Balance.Value);
        Assert.Equal(0, account.Version);
        Assert.Equal(now, account.UpdatedAtUtc);
    }

    [Fact]
    public void LargeBalancesHaveNoBusinessCeilingButCannotLosePrecision()
    {
        Account account = DomainTestData.CreateAccount(new AccountNumber("large"), 1000000000000000m);
        Assert.IsType<DepositResult.Success>(account.Deposit(new Amount(1m), account.CreatedAtUtc));
        Assert.Equal(1000000000000001m, account.Balance.Value);

        Account largest = DomainTestData.CreateAccount(new AccountNumber("largest"), decimal.MaxValue);
        Assert.IsType<DepositResult.Failure>(largest.Deposit(new Amount(1m), largest.CreatedAtUtc));
        Assert.IsType<DepositResult.Failure>(largest.Deposit(new Amount(0.0001m), largest.CreatedAtUtc));
        Assert.IsType<WithdrawResult.Failure>(largest.Withdraw(new Amount(0.0001m), largest.CreatedAtUtc));
        Assert.Equal(decimal.MaxValue, largest.Balance.Value);
        Assert.Equal(0, largest.Version);
    }
}
