using System.Data;

namespace Atm.Application.Abstractions.Persistence.Transactions;

public interface IPersistenceTransactionProvider
{
    ValueTask<IPersistenceTransaction> BeginTransactionAsync(
        IsolationLevel isolationLevel,
        CancellationToken cancellationToken);
}