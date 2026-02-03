namespace Atm.Application.Contracts.Accounts.CreateAccount;

public interface ICreateAccountPort
{
    CreateAccountResult Execute(CreateAccountRequest request);
}