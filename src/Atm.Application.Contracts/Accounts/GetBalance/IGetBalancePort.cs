namespace Atm.Application.Contracts.Accounts.GetBalance;

public interface IGetBalancePort
{
    GetBalanceResult Execute(GetBalanceRequest request);
}