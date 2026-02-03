using Atm.Domain.Operations;

namespace Atm.Application.Contracts.Operations.GetTransactionHistory;

public record TransactionDto(
    OperationType Type,
    decimal Amount,
    decimal BalanceAfter);