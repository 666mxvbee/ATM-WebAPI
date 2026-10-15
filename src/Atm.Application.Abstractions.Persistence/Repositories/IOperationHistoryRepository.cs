using Atm.Application.Models.Accounts.ValueObjects;
using Atm.Application.Models.Operations;

namespace Atm.Application.Abstractions.Persistence.Repositories;

public interface IOperationHistoryRepository
{
    IAsyncEnumerable<Operation> FindByAccountIdAsync(
        AccountId accountId,
        CancellationToken cancellationToken);
}