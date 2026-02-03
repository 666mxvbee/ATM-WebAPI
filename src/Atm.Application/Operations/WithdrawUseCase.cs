using Atm.Application.Abstractions.Repositories;
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

    public WithdrawUseCase(IAccountRepository accounts, IOperationRepository operations, SessionAuthorizer auth)
    {
        _accounts = accounts;
        _operations = operations;
        _auth = auth;
    }

    public WithdrawResult Execute(WithdrawRequest request)
    {
        AuthorizationResult auth = _auth.AuthorizeUser(request.UserSessionId);

        if (auth is AuthorizationResult.Unauthorized u)
        {
            return new WithdrawResult.Unauthorized(u.ErrorMessage);
        }

        var user = (AuthorizationResult.UserSuccess)auth;
        Domain.Accounts.Account? acc = _accounts.GetByNumber(user.AccountNumber);

        if (acc is null)
        {
            return new WithdrawResult.Failure("Account not found");
        }

        Amount amount;
        try
        {
            amount = new Amount(request.Amount);
        }
        catch (Exception e)
        {
            return new WithdrawResult.Failure(e.Message);
        }

        Domain.Accounts.Results.WithdrawResult domainResult = acc.Withdraw(amount);
        if (domainResult is Domain.Accounts.Results.WithdrawResult.Failure result)
        {
            return new WithdrawResult.Failure(result.ErrorMessage);
        }

        _accounts.Update(acc);
        _operations.Add(new OperationRecord(Guid.NewGuid(), user.AccountNumber, OperationType.Withdraw, amount, acc.Balance));

        return new WithdrawResult.Success(acc.Balance.Value);
    }
}