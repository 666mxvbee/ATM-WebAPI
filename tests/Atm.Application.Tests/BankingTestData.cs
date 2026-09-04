using Atm.Domain.Accounts;
using Atm.Domain.Accounts.Results;
using Atm.Domain.Sessions;
using Atm.Domain.Sessions.Results;
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

    public static Session CreateUserSession(Guid id, Account account, FixedClock clock)
    {
        return Assert.IsType<SessionCreationResult.Success>(Session.Create(
            id, SessionType.User, account.Id, clock.UtcNow, clock.UtcNow.AddMinutes(30))).Session;
    }

    public static Session CreateAdminSession(Guid id, FixedClock clock)
    {
        return Assert.IsType<SessionCreationResult.Success>(Session.Create(
            id, SessionType.Admin, null, clock.UtcNow, clock.UtcNow.AddMinutes(15))).Session;
    }
}
