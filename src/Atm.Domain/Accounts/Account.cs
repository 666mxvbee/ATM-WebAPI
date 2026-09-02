using Atm.Domain.Accounts.Results;
using Atm.Domain.Common;
using Atm.Domain.ValueObjects;

namespace Atm.Domain.Accounts;

public sealed class Account
{
    private Account() { }

    public Guid Id { get; private set; }
    public AccountNumber Number { get; private set; } = null!;
    public Guid OwnerUserId { get; private set; }
    public PinHash PinHash { get; private set; } = null!;
    public Money Balance { get; private set; } = null!;
    public long Version { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public static AccountCreationResult Create(
        Guid id,
        AccountNumber? number,
        Guid ownerUserId,
        PinHash? pinHash,
        Money? balance,
        DateTimeOffset createdAtUtc)
    {
        if (id == Guid.Empty || ownerUserId == Guid.Empty)
            return new AccountCreationResult.Failure("Account and owner IDs must not be empty.");
        if (number is null || pinHash is null || balance is null)
            return new AccountCreationResult.Failure("Number, PIN hash and balance are required.");
        if (!DomainRules.IsUtc(createdAtUtc))
            return new AccountCreationResult.Failure("Account timestamps must be UTC.");

        return new AccountCreationResult.Success(new Account
        {
            Id = id,
            Number = number,
            OwnerUserId = ownerUserId,
            PinHash = pinHash,
            Balance = balance,
            Version = 0,
            CreatedAtUtc = createdAtUtc,
            UpdatedAtUtc = createdAtUtc,
        });
    }

    public DepositResult Deposit(Amount? amount, DateTimeOffset updatedAtUtc)
    {
        string? error = ValidateChange(amount, updatedAtUtc);
        if (error is not null)
            return new DepositResult.Failure(error);

        Money balance;
        try
        {
            balance = Balance.Add(amount!);
        }
        catch (OverflowException)
        {
            return new DepositResult.Failure("The balance cannot be represented exactly as decimal.");
        }

        ApplyBalance(balance, updatedAtUtc);
        return new DepositResult.Success(Balance);
    }

    public WithdrawResult Withdraw(Amount? amount, DateTimeOffset updatedAtUtc)
    {
        string? error = ValidateChange(amount, updatedAtUtc);
        if (error is not null)
            return new WithdrawResult.Failure(error);
        if (Balance.Value < amount!.Value)
            return new WithdrawResult.Failure("Not enough balance.");

        Money balance;
        try
        {
            balance = Balance.Subtract(amount);
        }
        catch (OverflowException)
        {
            return new WithdrawResult.Failure("The balance cannot be represented exactly as decimal.");
        }

        ApplyBalance(balance, updatedAtUtc);
        return new WithdrawResult.Success(Balance);
    }

    private string? ValidateChange(Amount? amount, DateTimeOffset updatedAtUtc)
    {
        if (amount is null)
            return "Amount is required.";
        if (!DomainRules.IsUtc(updatedAtUtc) || updatedAtUtc < UpdatedAtUtc)
            return "Update time must be UTC and must not precede the previous update.";
        if (Version == long.MaxValue)
            return "Account version is exhausted.";
        return null;
    }

    private void ApplyBalance(Money balance, DateTimeOffset updatedAtUtc)
    {
        Balance = balance;
        Version++;
        UpdatedAtUtc = updatedAtUtc;
    }
}
