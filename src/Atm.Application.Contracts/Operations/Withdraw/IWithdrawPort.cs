namespace Atm.Application.Contracts.Operations.Withdraw;

public interface IWithdrawPort
{
    WithdrawResult Execute(WithdrawRequest request);
}