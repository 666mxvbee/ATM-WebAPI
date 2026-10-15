using Atm.Application.Contracts.Sessions.Models;

namespace Atm.Application.Contracts.Sessions.Operations;

public static class CreateAdminSession
{
    public readonly record struct Request(string SystemPassword);

    public abstract record Response
    {
        private Response() { }

        public sealed record Success(SessionDto Session) : Response;

        public sealed record Failure(string ErrorMessage) : Response;
    }
}