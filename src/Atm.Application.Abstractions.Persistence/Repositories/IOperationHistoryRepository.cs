using Atm.Application.Models.Accounts.ValueObjects;
using Atm.Application.Models.Operations;

namespace Atm.Application.Abstractions.Persistence.Repositories;

public interface IOperationHistoryRepository
{
    Task AddOperationAsync(
        Operation operation,
        CancellationToken cancellationToken);

    IAsyncEnumerable<Operation> FindByAccountIdAsync(
        AccountId accountId,
        CancellationToken cancellationToken);
}