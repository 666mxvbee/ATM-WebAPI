using Atm.Domain.Accounts;
using Atm.Domain.Accounts.Results;
using Atm.Domain.ValueObjects;

namespace Atm.Application.Tests;

internal static class BankingTestData
{
    public static Account CreateAccount(AccountNumber number, decimal balance)
    {
        return Assert.IsType<AccountCreationResult.Success>(Account.Create(
            Guid.NewGuid(), number, Guid.NewGuid(),
            new PinHash("test-only-hash"), new Money(balance), new FixedClock().UtcNow)).Account;
    }
}
