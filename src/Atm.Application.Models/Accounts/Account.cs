using Atm.Application.Models.Accounts.Results;
using Atm.Application.Models.Accounts.ValueObjects;

namespace Atm.Application.Models.Accounts;

public sealed class Account
{
    private Money _balance;

    public AccountId Id { get; }

    public AccountNumber Number { get; }

    public Pincode Pincode { get; }

    public Account(
        AccountId id,
        AccountNumber number,
        Pincode pincode,
        Money balance)
    {
        Id = id;
        Number = number;
        Pincode = pincode;
        _balance = balance;
    }

    public DepositResult Deposit(Money money)
    {
        _balance += money;

        return new DepositResult.Success();
    }

    public WithdrawResult Withdraw(Money money)
    {
        if (money > _balance)
        {
            return new WithdrawResult.Failure("Insufficient funds");
        }

        _balance -= money;

        return new WithdrawResult.Success();
    }
}