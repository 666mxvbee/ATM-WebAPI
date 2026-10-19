using Atm.Application.Models.Accounts.ValueObjects;

namespace Atm.Application.Models.Sessions;

public sealed class UserSession
{
    public Guid SessionId { get; }

    public AccountId AccountId { get; }

    public UserSession(
        Guid sessionId,
        AccountId accountId)
    {
        SessionId = sessionId;
        AccountId = accountId;
    }
}