namespace Atm.Application.Contracts.Accounts.Models;

public sealed record OperationHistoryDto(
    long OperationId,
    long AccountId,
    DateTimeOffset Time,
    string OperationType,
    decimal Amount,
    decimal BalanceAfter);