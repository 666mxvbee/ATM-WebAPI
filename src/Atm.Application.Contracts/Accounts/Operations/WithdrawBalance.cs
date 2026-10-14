using Atm.Application.Contracts.Accounts.Models;

namespace Atm.Application.Contracts.Accounts.Operations;

public static class WithdrawBalance
{
    public readonly record struct Request(Guid SessionId, decimal Money);

    public abstract record Response
    {
        public sealed record Success(BalanceDto Balance) : Response;

        public sealed record Failure(string ErrorMessage) : Response;
    }
}