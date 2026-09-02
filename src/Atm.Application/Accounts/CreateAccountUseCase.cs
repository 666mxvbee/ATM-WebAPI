using Atm.Application.Abstractions.Repositories;
using Atm.Application.Abstractions.Security;
using Atm.Application.Abstractions.Time;
using Atm.Application.Common;
using Atm.Application.Contracts.Accounts.CreateAccount;
using Atm.Domain.Accounts;
using Atm.Domain.Accounts.Results;
using Atm.Domain.ValueObjects;

namespace Atm.Application.Accounts;

internal sealed class CreateAccountUseCase : ICreateAccountPort
{
    private readonly IAccountRepository _accounts;
    private readonly SessionAuthorizer _auth;
    private readonly IPinHasher _pinHasher;
    private readonly IClock _clock;

    public CreateAccountUseCase(
        IAccountRepository accounts,
        SessionAuthorizer auth,
        IPinHasher pinHasher,
        IClock clock)
    {
        _accounts = accounts;
        _auth = auth;
        _pinHasher = pinHasher;
        _clock = clock;
    }

    public CreateAccountResult Execute(CreateAccountRequest request)
    {
        AuthorizationResult auth = _auth.AuthorizeAdmin(request.AdminSessionId);
        if (auth is AuthorizationResult.Unauthorized unauthorized)
            return new CreateAccountResult.Unauthorized(unauthorized.ErrorMessage);

        AccountNumber number;
        PinCode pin;
        Money balance;
        try
        {
            number = new AccountNumber(request.AccountNumber);
            pin = new PinCode(request.Pin);
            balance = new Money(request.InitialBalance);
        }
        catch (ArgumentException exception)
        {
            return new CreateAccountResult.Failure(exception.Message);
        }

        if (request.OwnerUserId == Guid.Empty)
            return new CreateAccountResult.Failure("Owner ID must not be empty.");
        if (_accounts.GetByNumber(number) is not null)
            return new CreateAccountResult.Failure("Account number already exists.");

        AccountCreationResult result = Account.Create(
            Guid.NewGuid(), number, request.OwnerUserId,
            _pinHasher.Hash(pin), balance, _clock.UtcNow);

        if (result is AccountCreationResult.Failure failure)
            return new CreateAccountResult.Failure(failure.ErrorMessage);

        _accounts.Add(((AccountCreationResult.Success)result).Account);
        return new CreateAccountResult.Success();
    }
}
