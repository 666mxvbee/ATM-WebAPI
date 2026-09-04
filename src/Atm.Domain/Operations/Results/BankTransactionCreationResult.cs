namespace Atm.Domain.Operations.Results;

public abstract record BankTransactionCreationResult
{
    private BankTransactionCreationResult() { }

    public sealed record Success(BankTransaction Transaction) : BankTransactionCreationResult;

    public sealed record Failure(string ErrorMessage) : BankTransactionCreationResult;
}
