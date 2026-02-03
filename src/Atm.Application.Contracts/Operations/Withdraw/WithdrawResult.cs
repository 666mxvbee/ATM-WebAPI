namespace Atm.Application.Contracts.Operations.Withdraw;

public abstract record WithdrawResult
{
    private WithdrawResult() { }

    public sealed record Success(decimal Balance) : WithdrawResult;

    public sealed record Unauthorized(string ErrorMessage) : WithdrawResult;

    public sealed record Failure(string ErrorMessage) : WithdrawResult;
}