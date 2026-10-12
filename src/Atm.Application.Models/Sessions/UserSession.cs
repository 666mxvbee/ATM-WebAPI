using Atm.Application.Models.Accounts.ValueObjects;

namespace Atm.Application.Models.Sessions;

public sealed class UserSession
{
    public UserSession(
        Guid sessionId,
        AccountId accountId)
    {
        SessionId = sessionId;
        AccountId = accountId;
    }

    public Guid SessionId { get; }

    public AccountId AccountId { get; }
}