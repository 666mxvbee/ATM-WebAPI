namespace Atm.Application.Contracts.Operations.Deposit;

public interface IDepositPort
{
    DepositResult Execute(DepositRequest request);
}