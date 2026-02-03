namespace Atm.Application.Contracts.Accounts.GetBalance;

public abstract record GetBalanceResult
{
    private GetBalanceResult() { }

    public sealed record Success(decimal Balance) : GetBalanceResult;

    public sealed record Unauthorized(string ErrorMessage) : GetBalanceResult;

    public sealed record Failure(string ErrorMessage) : GetBalanceResult;
}