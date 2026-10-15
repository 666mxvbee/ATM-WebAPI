using Atm.Application.Models.Accounts.Results;
using Atm.Application.Models.Accounts.ValueObjects;

namespace Atm.Application.Models.Accounts;

public sealed class Account
{
    private Money _balance;

    private Account(
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

    public static Account CreateFromService(
        AccountNumber number,
        Pincode pincode)
    {
        return new Account(
            AccountId.Default(),
            number,
            pincode,
            Money.Create(0));
    }

    public static Account CreateFromDb(
        AccountId id,
        AccountNumber number,
        Pincode pincode,
        Money balance)
    {
        return new Account(id, number, pincode, balance);
    }

    public AccountId Id { get; }

    public AccountNumber Number { get; }

    public Pincode Pincode { get; }

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