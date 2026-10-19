using Atm.Application.Abstractions.Repositories;
using Atm.Application.Models.Accounts.ValueObjects;
using Atm.Application.Models.Operations;
using Atm.Application.Models.Operations.ValueObjects;
using System.Runtime.CompilerServices;

namespace Atm.Infrastructure.Persistence.Repositories;

public sealed class OperationHistoryRepository : IOperationHistoryRepository
{
    private readonly Dictionary<OperationId, Operation> _values = [];
    private readonly Lock _sync = new();
    private long _lastId;

    public Task AddOperationAsync(
        Operation operation,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_sync)
        {
            var savedOperation = Operation.CreateFromDb(
                OperationId.Create(++_lastId),
                operation.AccountId,
                operation.Time,
                operation.OperationType,
                operation.Amount,
                operation.BalanceAfter);

            _values.Add(savedOperation.OperationId, savedOperation);
        }

        return Task.CompletedTask;
    }

    public async IAsyncEnumerable<Operation> FindByAccountIdAsync(
        AccountId accountId,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        Operation[] operations;

        lock (_sync)
        {
            operations =
            [
                .. _values.Values
                    .Where(x => x.AccountId == accountId)
                    .OrderByDescending(x => x.Time)
                    .ThenByDescending(x => x.OperationId.Value)
            ];
        }

        await Task.CompletedTask;

        foreach (Operation operation in operations)
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return operation;
        }
    }
}