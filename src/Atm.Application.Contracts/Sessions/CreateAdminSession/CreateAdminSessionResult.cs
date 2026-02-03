namespace Atm.Application.Contracts.Sessions.CreateAdminSession;

public abstract record CreateAdminSessionResult
{
    private CreateAdminSessionResult() { }

    public sealed record Success(Guid SessionId) : CreateAdminSessionResult;

    public sealed record Unauthorized(string ErrorMessage) : CreateAdminSessionResult;

    public sealed record Failure(string ErrorMessage) : CreateAdminSessionResult;
}