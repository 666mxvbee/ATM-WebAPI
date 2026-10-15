using Atm.Application.Models.Sessions;

namespace Atm.Application.Abstractions.Persistence.Repositories;

public interface IUserSessionRepository
{
    Task<UserSession> AddSessionsAsync(
        IReadOnlyCollection<UserSession> sessions,
        CancellationToken cancellationToken);

    Task<UserSession> GetUserSessionByIdAsync(
        Guid sessionId,
        CancellationToken cancellationToken);
}