using Atm.Application.Contracts.Accounts.Operations;

namespace Atm.Application.Contracts.Accounts;

public interface IAccountService
{
    Task<GetBalance.Response> GetAccountBalanceAsync(
        GetBalance.Request request,
        CancellationToken cancellationToken);

    Task<WithdrawBalance.Response> WithdrawBalanceAsync(
        WithdrawBalance.Request request,
        CancellationToken cancellationToken);

    Task<DepositBalance.Response> DepositBalanceAsync(
        WithdrawBalance.Request request,
        CancellationToken cancellationToken);

    Task<GetOperationHistory.Response> GetOperationHistoryAsync(
        GetOperationHistory.Request request,
        CancellationToken cancellationToken);
}