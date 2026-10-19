namespace Atm.Application.Models.Accounts.Results;

public abstract record WithdrawResult
{
    private WithdrawResult() { }

    public sealed record Success : WithdrawResult;

    public sealed record Failure(string ErrorMessage) : WithdrawResult;
}