using Atm.Application.Abstractions.Repositories;
using Atm.Domain.Sessions;

namespace Atm.Application.Common;

public sealed class SessionAuthorizer
{
    private readonly ISessionRepository _sessions;

    public SessionAuthorizer(ISessionRepository sessions)
    {
        _sessions = sessions;
    }

    public AuthorizationResult AuthorizeUser(Guid sessionId)
    {
        Session? s = _sessions.GetById(sessionId);

        if (s is null)
        {
            return new AuthorizationResult.Unauthorized("Session not found");
        }

        if (s.Type != SessionType.User)
        {
            return new AuthorizationResult.Unauthorized("Session type not supported [user session required]");
        }

        if (s.AccountNumber is null)
        {
            return new AuthorizationResult.Unauthorized("Account number not set");
        }

        return new AuthorizationResult.UserSuccess(s.AccountNumber);
    }

    public AuthorizationResult AuthorizeAdmin(Guid sessionId)
    {
        Session? s = _sessions.GetById(sessionId);

        if (s is null)
        {
            return new AuthorizationResult.Unauthorized("Session not found");
        }

        if (s.Type != SessionType.Admin)
        {
            return new AuthorizationResult.Unauthorized("Session type not supported [admin session required]");
        }

        return new AuthorizationResult.AdminSuccess();
    }
}