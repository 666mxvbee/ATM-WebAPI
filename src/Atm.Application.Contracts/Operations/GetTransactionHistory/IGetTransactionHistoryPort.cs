namespace Atm.Application.Contracts.Operations.GetTransactionHistory;

public interface IGetTransactionHistoryPort
{
    GetTransactionHistoryResult Execute(GetTransactionHistoryRequest request);
}