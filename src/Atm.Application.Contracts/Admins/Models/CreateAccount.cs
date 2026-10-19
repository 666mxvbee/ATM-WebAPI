using Atm.Application.Contracts.Admins.Operations;

namespace Atm.Application.Contracts.Admins.Models;

public static class CreateAccount
{
    public readonly record struct Request(
        Guid SessionId,
        string AccountNumber,
        string PinCode);

    public abstract record Response
    {
        private Response() { }

        public sealed record Success(AccountDto Account) : Response;

        public sealed record Failure(string ErrorMessage) : Response;
    }
}