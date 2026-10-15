using Atm.Application.Abstractions.Persistence.Repositories;

namespace Atm.Application.Abstractions.Persistence;

public interface IPersistenceContext
{
    IAccountRepository AccountRepository { get; }

    IAdminSessionRepository AdminSessionRepository { get; }

    IUserSessionRepository UserSessionRepository { get; }

    IOperationHistoryRepository OperationHistoryRepository { get; }
}