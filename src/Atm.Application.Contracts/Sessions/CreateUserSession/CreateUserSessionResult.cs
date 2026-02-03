namespace Atm.Application.Contracts.Sessions.CreateUserSession;

public abstract record CreateUserSessionResult
{
    private CreateUserSessionResult() { }

    public sealed record Success(Guid SessionId) : CreateUserSessionResult;

    public sealed record Unauthorized(string ErrorMessage) : CreateUserSessionResult;

    public sealed record Failure(string ErrorMessage) : CreateUserSessionResult;
}