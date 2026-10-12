using Atm.Application.Contracts.Sessions.Models;
using Atm.Application.Models.Accounts.ValueObjects;

namespace Atm.Application.Contracts.Sessions.Operations;

public static class CreateUserSession
{
    public readonly record struct Request(
        AccountNumber AccountNumber,
        Pincode Pincode);

    public abstract record Response
    {
        private Response() { }

        public sealed record Success(SessionDto Session) : Response;

        public sealed record Failure(string ErrorMessage) : Response;
    }
}