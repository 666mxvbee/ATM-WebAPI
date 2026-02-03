using Atm.Domain.ValueObjects;

namespace Atm.Application.Common;

public abstract record AuthorizationResult
{
    private AuthorizationResult() { }

    public sealed record UserSuccess(AccountNumber AccountNumber) : AuthorizationResult;

    public sealed record AdminSuccess : AuthorizationResult;

    public sealed record Unauthorized(string ErrorMessage) : AuthorizationResult;
}