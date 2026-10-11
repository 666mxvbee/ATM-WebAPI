namespace Atm.Application.Models.Accounts.ValueObjects;

public readonly record struct AccountId
{
    public long Value { get; }

    private AccountId(long value)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(value);

        Value = value;
    }

    public static AccountId Create(long value)
    {
        return new AccountId(value);
    }
}