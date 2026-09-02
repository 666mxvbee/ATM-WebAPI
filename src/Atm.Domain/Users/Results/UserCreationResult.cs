namespace Atm.Domain.Users.Results;

public abstract record UserCreationResult
{
    private UserCreationResult() { }

    public sealed record Success(User User) : UserCreationResult;

    public sealed record Failure(string ErrorMessage) : UserCreationResult;
}
