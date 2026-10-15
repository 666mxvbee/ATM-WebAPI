namespace Atm.Application.Contracts.Accounts.Models;

public sealed record OperationHistoryDto(
    Guid OperationId,
    Guid AccountId,
    OperationType OperationType,
    decimal Amount,
    decimal BalanceAfter);