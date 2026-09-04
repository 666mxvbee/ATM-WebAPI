namespace Atm.Domain.Sessions.Results;

public abstract record SessionRevocationResult
{
    private SessionRevocationResult() { }

    public sealed record Success(DateTimeOffset RevokedAtUtc) : SessionRevocationResult;

    public sealed record Failure(string ErrorMessage) : SessionRevocationResult;
}
