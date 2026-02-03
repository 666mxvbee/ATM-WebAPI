using Atm.Domain.ValueObjects;

namespace Atm.Domain.Accounts.Results;

public abstract record WithdrawResult
{
    private WithdrawResult() { }

    public sealed record Success(Money BalanceAfter) : WithdrawResult;

    public sealed record Failure(string ErrorMessage) : WithdrawResult;
}