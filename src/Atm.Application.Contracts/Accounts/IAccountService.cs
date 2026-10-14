using Atm.Application.Contracts.Accounts.Operations;

namespace Atm.Application.Contracts.Accounts;

public interface IAccountService
{
    Task<CreateAccount.Response> CreateAccountAsync(
        CreateAccount.Request request,
        CancellationToken cancellationToken);

    Task<GetBalance.Response> GetAccountAsync(
        GetBalance.Request request,
        CancellationToken cancellationToken);

    Task<WithdrawBalance.Response> WithdrawBalanceAsync(
        WithdrawBalance.Request request,
        CancellationToken cancellationToken);

    Task<DepositBalance.Response> DepositBalanceAsync(
        WithdrawBalance.Request request,
        CancellationToken cancellationToken);
}