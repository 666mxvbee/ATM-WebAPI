using Atm.Application.Abstractions.Repositories;
using Atm.Application.Abstractions.Time;
using Atm.Application.Common;
using Atm.Application.Contracts.Operations.Withdraw;
using Atm.Domain.Operations;
using Atm.Domain.ValueObjects;

namespace Atm.Application.Operations;

public sealed class WithdrawUseCase : IWithdrawPort
{
    private readonly IAccountRepository _accounts;
    private readonly IOperationRepository _operations;
    private readonly SessionAuthorizer _auth;
    private readonly IClock _clock;

    public WithdrawUseCase(IAccountRepository accounts, IOperationRepository operations, SessionAuthorizer auth, IClock clock)
    {
        _accounts = accounts;
        _operations = operations;
        _auth = auth;
        _clock = clock;
    }

    public WithdrawResult Execute(WithdrawRequest request)
    {
        AuthorizationResult auth = _auth.AuthorizeUser(request.UserSessionId);

        if (auth is AuthorizationResult.Unauthorized u)
        {
            return new WithdrawResult.Unauthorized(u.ErrorMessage);
        }

        var user = (AuthorizationResult.UserSuccess)auth;
        Domain.Accounts.Account? acc = _accounts.GetById(user.AccountId);

        if (acc is null)
        {
            return new WithdrawResult.Failure("Account not found");
        }

        Amount amount;
        try
        {
            amount = new Amount(request.Amount);
        }
        catch (ArgumentException e)
        {
            return new WithdrawResult.Failure(e.Message);
        }

        Domain.Accounts.Results.WithdrawResult domainResult = acc.Withdraw(amount, _clock.UtcNow);
        if (domainResult is Domain.Accounts.Results.WithdrawResult.Failure result)
        {
            return new WithdrawResult.Failure(result.ErrorMessage);
        }

        _accounts.Update(acc);
        _operations.Add(new OperationRecord(Guid.NewGuid(), acc.Number, OperationType.Withdraw, amount, acc.Balance));

        return new WithdrawResult.Success(acc.Balance.Value);
    }
}
