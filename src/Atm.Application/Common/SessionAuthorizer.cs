using Atm.Application.Abstractions.Repositories;
using Atm.Application.Abstractions.Time;
using Atm.Domain.Sessions;

namespace Atm.Application.Common;

public sealed class SessionAuthorizer
{
    private readonly ISessionRepository _sessions;
    private readonly IClock _clock;

    public SessionAuthorizer(ISessionRepository sessions, IClock clock)
    {
        _sessions = sessions;
        _clock = clock;
    }

    public AuthorizationResult AuthorizeUser(Guid sessionId)
    {
        Session? session = _sessions.GetById(sessionId);
        if (session is null || !session.IsActive(_clock.UtcNow))
            return new AuthorizationResult.Unauthorized("Session is missing, expired or revoked.");
        if (session.Type != SessionType.User)
            return new AuthorizationResult.Unauthorized("Session type not supported [user session required]");
        if (session.AccountId is not Guid accountId)
            return new AuthorizationResult.Unauthorized("Account is not set.");

        return new AuthorizationResult.UserSuccess(accountId);
    }

    public AuthorizationResult AuthorizeAdmin(Guid sessionId)
    {
        Session? session = _sessions.GetById(sessionId);
        if (session is null || !session.IsActive(_clock.UtcNow))
            return new AuthorizationResult.Unauthorized("Session is missing, expired or revoked.");
        if (session.Type != SessionType.Admin)
            return new AuthorizationResult.Unauthorized("Session type not supported [admin session required]");

        return new AuthorizationResult.AdminSuccess();
    }
}
