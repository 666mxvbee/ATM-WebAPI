namespace Atm.Domain.Users.Results;

public abstract record ProfileUpdateResult
{
    private ProfileUpdateResult() { }

    public sealed record Success : ProfileUpdateResult;

    public sealed record Failure(string ErrorMessage) : ProfileUpdateResult;
}
