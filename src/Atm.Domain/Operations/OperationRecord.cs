using Atm.Domain.ValueObjects;

namespace Atm.Domain.Operations;

public sealed class OperationRecord
{
    public OperationRecord(
        Guid id,
        AccountNumber accountNumber,
        OperationType type,
        Amount amount,
        Money balanceAfter)
    {
        Id = id;
        AccountNumber = accountNumber;
        Type = type;
        Amount = amount;
        BalanceAfter = balanceAfter;
    }

    public Guid Id { get; }

    public AccountNumber AccountNumber { get; }

    public OperationType Type { get; }

    public Amount Amount { get; }

    public Money BalanceAfter { get; }
}