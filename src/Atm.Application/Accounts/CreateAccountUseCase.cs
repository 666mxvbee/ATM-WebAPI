using Atm.Application.Abstractions.Repositories;
using Atm.Application.Common;
using Atm.Application.Contracts.Accounts.CreateAccount;
using Atm.Domain.Accounts;
using Atm.Domain.ValueObjects;

namespace Atm.Application.Accounts;

internal sealed class CreateAccountUseCase : ICreateAccountPort
{
    private readonly IAccountRepository _accounts;
    private readonly SessionAuthorizer _auth;

    public CreateAccountUseCase(IAccountRepository accounts, ISessionRepository sessions)
    {
        _accounts = accounts;
        _auth = new SessionAuthorizer(sessions);
    }

    public CreateAccountResult Execute(CreateAccountRequest request)
    {
        AuthorizationResult auth = _auth.AuthorizeAdmin(request.AdminSessionId);

        if (auth is AuthorizationResult.Unauthorized u)
        {
            return new CreateAccountResult.Unauthorized(u.ErrorMessage);
        }

        try
        {
            var number = new AccountNumber(request.AccountNumber);
            var pin = new PinCode(request.Pin);
            var balance = new Money(request.InitialBalance);

            Account? existing = _accounts.GetByNumber(number);

            if (existing is not null)
            {
                return new CreateAccountResult.Failure("Account number already exists");
            }

            var account = new Account(number, pin, balance);
            _accounts.Add(account);

            return new CreateAccountResult.Success();
        }
        catch (Exception e)
        {
            return new CreateAccountResult.Failure(e.Message);
        }
    }
}