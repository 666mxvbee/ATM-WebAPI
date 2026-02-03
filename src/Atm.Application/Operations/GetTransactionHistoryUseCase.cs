using Atm.Application.Abstractions.Repositories;
using Atm.Application.Common;
using Atm.Application.Contracts.Operations.GetTransactionHistory;

namespace Atm.Application.Operations;

internal sealed class GetTransactionHistoryUseCase : IGetTransactionHistoryPort
{
    private readonly IOperationRepository _operations;
    private readonly SessionAuthorizer _auth;

    public GetTransactionHistoryUseCase(IOperationRepository operations, SessionAuthorizer auth)
    {
        _operations = operations;
        _auth = auth;
    }

    public GetTransactionHistoryResult Execute(GetTransactionHistoryRequest request)
    {
        AuthorizationResult auth = _auth.AuthorizeUser(request.UserSessionId);

        if (auth is AuthorizationResult.Unauthorized u)
        {
            return new GetTransactionHistoryResult.Unauthorized(u.ErrorMessage);
        }

        var user = (AuthorizationResult.UserSuccess)auth;
        IReadOnlyList<Domain.Operations.OperationRecord> ops = _operations.GetByAccountNumber(user.AccountNumber);
        var list = new List<TransactionDto>(ops.Count);

        foreach (Domain.Operations.OperationRecord op in ops)
        {
            list.Add(new TransactionDto(op.Type, op.Amount.Value, op.BalanceAfter.Value));
        }

        return new GetTransactionHistoryResult.Success(list);
    }
}