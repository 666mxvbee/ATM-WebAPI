using Atm.Application.Abstractions.Repositories;
using Atm.Application.Common;
using Atm.Application.Contracts.Operations.GetTransactionHistory;

namespace Atm.Application.Operations;

internal sealed class GetTransactionHistoryUseCase : IGetTransactionHistoryPort
{
    private readonly IOperationRepository _operations;
    private readonly IAccountRepository _accounts;
    private readonly SessionAuthorizer _auth;

    public GetTransactionHistoryUseCase(
        IOperationRepository operations,
        IAccountRepository accounts,
        SessionAuthorizer auth)
    {
        _operations = operations;
        _accounts = accounts;
        _auth = auth;
    }

    public GetTransactionHistoryResult Execute(GetTransactionHistoryRequest request)
    {
        AuthorizationResult auth = _auth.AuthorizeUser(request.UserSessionId);
        if (auth is AuthorizationResult.Unauthorized unauthorized)
            return new GetTransactionHistoryResult.Unauthorized(unauthorized.ErrorMessage);

        var user = (AuthorizationResult.UserSuccess)auth;
        Domain.Accounts.Account? account = _accounts.GetById(user.AccountId);
        if (account is null)
            return new GetTransactionHistoryResult.Failure("Account not found");

        IReadOnlyList<Domain.Operations.OperationRecord> operations =
            _operations.GetByAccountNumber(account.Number);
        var transactions = new List<TransactionDto>(operations.Count);
        foreach (Domain.Operations.OperationRecord operation in operations)
        {
            transactions.Add(new TransactionDto(
                operation.Type.ToString(),
                operation.Amount.Value,
                operation.BalanceAfter.Value));
        }

        return new GetTransactionHistoryResult.Success(transactions);
    }
}
