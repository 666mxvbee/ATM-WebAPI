using Atm.Application.Models.Sessions;

namespace Atm.Application.Abstractions.Persistence.Repositories;

public interface IUserSessionRepository
{
    Task<UserSession> AddSessionAsync(
        UserSession session,
        CancellationToken cancellationToken);

    Task<UserSession?> FindUserSessionByIdAsync(
        Guid sessionId,
        CancellationToken cancellationToken);
}