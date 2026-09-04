using Atm.Application.Abstractions.Repositories;
using Atm.Application.Common;
using Atm.Application.Contracts.Accounts.GetBalance;

namespace Atm.Application.Accounts;

internal sealed class GetBalanceUseCase : IGetBalancePort
{
    private readonly IAccountRepository _accounts;
    private readonly SessionAuthorizer _auth;

    public GetBalanceUseCase(IAccountRepository accounts, SessionAuthorizer auth)
    {
        _accounts = accounts;
        _auth = auth;
    }

    public GetBalanceResult Execute(GetBalanceRequest request)
    {
        AuthorizationResult auth = _auth.AuthorizeUser(request.UserSessionId);
        if (auth is AuthorizationResult.Unauthorized unauthorized)
            return new GetBalanceResult.Unauthorized(unauthorized.ErrorMessage);

        var user = (AuthorizationResult.UserSuccess)auth;
        Domain.Accounts.Account? account = _accounts.GetById(user.AccountId);
        if (account is null)
            return new GetBalanceResult.Failure("Account not found");

        return new GetBalanceResult.Success(account.Balance.Value);
    }
}
