using Atm.Application.Abstractions.Repositories;
using Atm.Application.Models.Sessions;
using System.Collections.Concurrent;

namespace Atm.Infrastructure.Persistence.Repositories;

public class UserSessionRepository : IUserSessionRepository
{
    private readonly ConcurrentDictionary<Guid, UserSession> _values = new();

    public Task<UserSession> AddSessionAsync(
        UserSession session,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (_values.TryAdd(session.SessionId, session) is false)
        {
            throw new InvalidOperationException("Session is already exists.");
        }

        return Task.FromResult(session);
    }

    public Task<UserSession?> FindUserSessionByIdAsync(
        Guid sessionId,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        _values.TryGetValue(sessionId, out UserSession? session);

        return Task.FromResult(session);
    }
}