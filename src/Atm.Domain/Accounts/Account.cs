using Atm.Domain.Accounts.Results;
using Atm.Domain.ValueObjects;

namespace Atm.Domain.Accounts;

public sealed class Account
{
    public Account(AccountNumber number, PinCode pin, Money balance)
    {
        Number = number;
        Pin = pin;
        Balance = balance;
    }

    public AccountNumber Number { get; }

    public PinCode Pin { get; }

    public Money Balance { get; private set; }

    public DepositResult Deposit(Amount amount)
    {
        Balance = new Money(Balance.Value + amount.Value);
        return new DepositResult.Success(Balance);
    }

    public WithdrawResult Withdraw(Amount amount)
    {
        if (Balance.Value < amount.Value)
        {
            return new WithdrawResult.Failure("Not enough balance");
        }

        Balance = new Money(Balance.Value - amount.Value);
        return new WithdrawResult.Success(Balance);
    }
}