namespace Atm.Application.Contracts.Accounts.Models;

public sealed record OperationHistoryDto(
    long OperationId,
    long AccountId,
    DateTimeOffset Time,
    OperationType OperationType,
    decimal Amount,
    decimal BalanceAfter);