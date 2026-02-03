namespace Atm.Application.Contracts.Operations.Deposit;

public abstract record DepositResult
{
    private DepositResult() { }

    public sealed record Success(decimal Balance) : DepositResult;

    public sealed record Unauthorized(string ErrorMessage) : DepositResult;

    public sealed record Failure(string ErrorMessage) : DepositResult;
}