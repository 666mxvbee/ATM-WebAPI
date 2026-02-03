using Atm.Domain.Sessions;

namespace Atm.Application.Abstractions.Repositories;

public interface ISessionRepository
{
    Session? GetById(Guid id);

    void Add(Session session);
}