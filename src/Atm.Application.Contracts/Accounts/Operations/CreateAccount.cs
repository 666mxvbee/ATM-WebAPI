using Atm.Application.Contracts.Accounts.Models;
using Atm.Application.Models.Accounts.ValueObjects;

namespace Atm.Application.Contracts.Accounts.Operations;

public static class CreateAccount
{
    public readonly record struct Request(
        Guid SessionId,
        AccountNumber AccountNumber,
        Pincode Pincode);

    public abstract record Response
    {
        private Response() { }

        public sealed record Success(AccountDto AccountNumber) : Response;

        public sealed record Failure(string ErrorMessage) : Response;
    }
}