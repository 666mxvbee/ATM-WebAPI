namespace Atm.Application.Models.Accounts.Results;

public abstract record DepositResult
{
    private DepositResult() { }

    public sealed record Success : DepositResult;
}