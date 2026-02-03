namespace Atm.Application.Contracts.Accounts.CreateAccount;

public abstract record CreateAccountResult
{
    private CreateAccountResult() { }

    public sealed record Success : CreateAccountResult;

    public sealed record Unauthorized(string ErrorMessage) : CreateAccountResult;

    public sealed record Failure(string ErrorMessage) : CreateAccountResult;
}