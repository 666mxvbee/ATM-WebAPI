namespace Atm.Application.Contracts.Operations.GetTransactionHistory;

public record TransactionDto(
    string Type,
    decimal Amount,
    decimal BalanceAfter);