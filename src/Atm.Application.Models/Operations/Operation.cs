using Atm.Application.Models.Accounts.ValueObjects;
using Atm.Application.Models.Operations.ValueObjects;

namespace Atm.Application.Models.Operations;

public sealed class Operation
{
    private Operation(
        OperationId operationId,
        AccountId accountId,
        DateTimeOffset time,
        OperationType operationType,
        Money amount,
        Money balanceAfter)
    {
        OperationId = operationId;
        AccountId = accountId;
        Time = time;
        OperationType = operationType;
        Amount = amount;
        BalanceAfter = balanceAfter;
    }

    public static Operation CreateFromService(
        AccountId accountId,
        DateTimeOffset time,
        OperationType operationType,
        Money amount,
        Money balanceAfter)
    {
        return new Operation(
            OperationId.Default(),
            accountId,
            time,
            operationType,
            amount,
            balanceAfter);
    }

    public static Operation CreateFromDb(
        OperationId operationId,
        AccountId accountId,
        DateTimeOffset time,
        OperationType operationType,
        Money amount,
        Money balanceAfter)
    {
        return new Operation(
            operationId,
            accountId,
            time,
            operationType,
            amount,
            balanceAfter);
    }

    public OperationId OperationId { get; }

    public AccountId AccountId { get; }

    public DateTimeOffset Time { get; }

    public OperationType OperationType { get; }

    public Money Amount { get; }

    public Money BalanceAfter { get; }
}