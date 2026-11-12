using Atm.Application.Models.Sessions;

namespace Atm.Application.Abstractions.Repositories;

public interface IAdminSessionRepository
{
    IAsyncEnumerable<AdminSession> AddSessionAsync(
        IReadOnlyCollection<AdminSession> sessions,
        CancellationToken cancellationToken);

    Task<AdminSession?> FindAdminSessionByIdAsync(
        Guid sessionId,
        CancellationToken cancellationToken);
}