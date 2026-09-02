namespace Atm.Domain.Accounts.Results;

public abstract record AccountCreationResult
{
    private AccountCreationResult() { }

    public sealed record Success(Account Account) : AccountCreationResult;

    public sealed record Failure(string ErrorMessage) : AccountCreationResult;
}
