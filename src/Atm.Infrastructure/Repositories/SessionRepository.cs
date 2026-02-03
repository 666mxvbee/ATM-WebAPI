using Atm.Application.Abstractions.Repositories;
using Atm.Domain.Sessions;

namespace Atm.Infrastructure.Repositories;

public sealed class SessionRepository : ISessionRepository
{
    private readonly Dictionary<Guid, Session> _sessions = new();

    public Session? GetById(Guid id)
    {
        if (_sessions.TryGetValue(id, out Session? session))
        {
            return session;
        }

        return null;
    }

    public void Add(Session session)
    {
        _sessions.Add(session.Id, session);
    }
}