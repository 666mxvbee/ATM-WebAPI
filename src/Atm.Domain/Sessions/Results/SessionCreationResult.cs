namespace Atm.Domain.Sessions.Results;

public abstract record SessionCreationResult
{
    private SessionCreationResult() { }

    public sealed record Success(Session Session) : SessionCreationResult;

    public sealed record Failure(string ErrorMessage) : SessionCreationResult;
}
