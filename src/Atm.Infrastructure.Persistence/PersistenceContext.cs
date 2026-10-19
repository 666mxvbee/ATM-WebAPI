using Atm.Application.Abstractions;
using Atm.Application.Abstractions.Repositories;

namespace Atm.Infrastructure.Persistence;

public sealed class PersistenceContext : IPersistenceContext
{
    public IAccountRepository AccountRepository { get; }

    public IAdminSessionRepository AdminSessionRepository { get; }

    public IUserSessionRepository UserSessionRepository { get; }

    public IOperationHistoryRepository OperationHistoryRepository { get; }

    public PersistenceContext(
        IAccountRepository accountRepository,
        IAdminSessionRepository adminSessionRepository,
        IUserSessionRepository userSessionRepository,
        IOperationHistoryRepository operationHistoryRepository)
    {
        AccountRepository = accountRepository;
        AdminSessionRepository = adminSessionRepository;
        UserSessionRepository = userSessionRepository;
        OperationHistoryRepository = operationHistoryRepository;
    }
}