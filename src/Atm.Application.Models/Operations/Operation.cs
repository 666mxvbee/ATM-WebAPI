using Atm.Application.Models.Accounts.ValueObjects;
using Atm.Application.Models.Operations.ValueObjects;

namespace Atm.Application.Models.Operations;

public sealed class Operation
{
    public OperationId OperationId { get; }

    public AccountId AccountId { get; }

    public DateTimeOffset Time { get; }

    public OperationType OperationType { get; }

    public Money Balance { get; }

    public Operation(
        OperationId operationId,
        AccountId accountId,
        DateTimeOffset time,
        OperationType operationType,
        Money balance)
    {
        OperationId = operationId;
        AccountId = accountId;
        Time = time;
        OperationType = operationType;
        Balance = balance;
    }
}