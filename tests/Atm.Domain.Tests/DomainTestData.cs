using Atm.Domain.Accounts;
using Atm.Domain.Accounts.Results;
using Atm.Domain.Sessions;
using Atm.Domain.Sessions.Results;
using Atm.Domain.ValueObjects;

namespace Atm.Domain.Tests;

internal static class DomainTestData
{
    public static DateTimeOffset UtcNow { get; } =
        new(2026, 9, 2, 12, 0, 0, TimeSpan.Zero);

    public static Account CreateAccount(AccountNumber number, decimal balance)
    {
        return Assert.IsType<AccountCreationResult.Success>(Account.Create(
            Guid.NewGuid(),
            number,
            Guid.NewGuid(),
            new PinHash("test-only-hash"),
            new Money(balance),
            UtcNow)).Account;
    }

    public static Session CreateAdminSession(Guid id)
    {
        return Assert.IsType<SessionCreationResult.Success>(Session.Create(
            id,
            SessionType.Admin,
            null,
            UtcNow,
            UtcNow.AddMinutes(15))).Session;
    }
}
