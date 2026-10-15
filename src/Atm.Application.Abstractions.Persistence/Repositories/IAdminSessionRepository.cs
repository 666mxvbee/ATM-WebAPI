using Atm.Application.Models.Sessions;

namespace Atm.Application.Abstractions.Persistence.Repositories;

public interface IAdminSessionRepository
{
    Task<AdminSession> AddSessionsAsync(
        IReadOnlyCollection<AdminSession> sessions,
        CancellationToken cancellationToken);

    Task<AdminSession> GetAdminSessionByIdAsync(
        Guid sessionId,
        CancellationToken cancellationToken);
}