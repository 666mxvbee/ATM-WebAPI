using Atm.Application.Contracts.Accounts.Models;

namespace Atm.Application.Contracts.Accounts.Operations;

public static class GetOperationHistory
{
    public readonly record struct Request(Guid SessionId);

    public abstract record Response
    {
        private Response() { }

        public sealed record Success(
            IReadOnlyCollection<OperationHistoryDto> OperationHistory) : Response;

        public sealed record Failure(string ErrorMessage) : Response;
    }
}