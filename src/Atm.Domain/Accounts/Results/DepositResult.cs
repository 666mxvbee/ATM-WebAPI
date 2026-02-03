using Atm.Domain.ValueObjects;

namespace Atm.Domain.Accounts.Results;

public abstract record DepositResult
{
    private DepositResult() { }

    public sealed record Success(Money BalanceAfter) : DepositResult;

    public sealed record Failure(string ErrorMessage) : DepositResult;
}