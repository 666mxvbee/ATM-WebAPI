using Atm.Application.Abstractions.Repositories;
using Atm.Application.Models.Sessions;
using System.Collections.Concurrent;

namespace Atm.Infrastructure.Persistence.Repositories;

public sealed class AdminSessionRepository : IAdminSessionRepository
{
    private readonly ConcurrentDictionary<Guid, AdminSession> _values = new();

    public Task<AdminSession> AddSessionAsync(
        AdminSession sessions,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (_values.TryAdd(sessions.SessionId, sessions) is false)
        {
            throw new InvalidOperationException("Session already exists");
        }

        return Task.FromResult(sessions);
    }

    public Task<AdminSession?> FindAdminSessionByIdAsync(
        Guid sessionId,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        _values.TryGetValue(sessionId, out AdminSession? session);

        return Task.FromResult(session);
    }
}