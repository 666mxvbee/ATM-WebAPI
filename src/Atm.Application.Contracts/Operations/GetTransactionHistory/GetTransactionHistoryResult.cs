namespace Atm.Application.Contracts.Operations.GetTransactionHistory;

public abstract record GetTransactionHistoryResult
{
    private GetTransactionHistoryResult() { }

    public sealed record Success(IReadOnlyList<TransactionDto> Transactions) : GetTransactionHistoryResult;

    public sealed record Unauthorized(string ErrorMessage) : GetTransactionHistoryResult;

    public sealed record Failure(string ErrorMessage) : GetTransactionHistoryResult;
}