using Atm.Application.Abstractions.Repositories;
using Atm.Application.Abstractions.Time;
using Atm.Application.Common;
using Atm.Application.Contracts.Operations.Deposit;
using Atm.Domain.Operations;
using Atm.Domain.ValueObjects;

namespace Atm.Application.Operations;

public sealed class DepositUseCase : IDepositPort
{
    private readonly IAccountRepository _accounts;
    private readonly IOperationRepository _operations;
    private readonly SessionAuthorizer _auth;
    private readonly IClock _clock;

    public DepositUseCase(IAccountRepository accounts, IOperationRepository operations, SessionAuthorizer auth, IClock clock)
    {
        _accounts = accounts;
        _operations = operations;
        _auth = auth;
        _clock = clock;
    }

    public DepositResult Execute(DepositRequest request)
    {
        AuthorizationResult auth = _auth.AuthorizeUser(request.UserSessionId);

        if (auth is AuthorizationResult.Unauthorized u)
        {
            return new DepositResult.Unauthorized(u.ErrorMessage);
        }

        var user = (AuthorizationResult.UserSuccess)auth;
        Domain.Accounts.Account? acc = _accounts.GetByNumber(user.AccountNumber);

        if (acc is null)
        {
            return new DepositResult.Failure("Account not found");
        }

        Amount amount;
        try
        {
            amount = new Amount(request.Amount);
        }
        catch (ArgumentException e)
        {
            return new DepositResult.Failure(e.Message);
        }

        Domain.Accounts.Results.DepositResult domainResult = acc.Deposit(amount, _clock.UtcNow);

        if (domainResult is Domain.Accounts.Results.DepositResult.Failure f)
        {
            return new DepositResult.Failure(f.ErrorMessage);
        }

        _accounts.Update(acc);
        _operations.Add(new OperationRecord(Guid.NewGuid(), user.AccountNumber, OperationType.Deposit, amount, acc.Balance));

        return new DepositResult.Success(acc.Balance.Value);
    }
}
