namespace Atm.Application.Common;

public abstract record AuthorizationResult
{
    private AuthorizationResult() { }

    public sealed record UserSuccess(Guid AccountId) : AuthorizationResult;

    public sealed record AdminSuccess : AuthorizationResult;

    public sealed record Unauthorized(string ErrorMessage) : AuthorizationResult;
}
