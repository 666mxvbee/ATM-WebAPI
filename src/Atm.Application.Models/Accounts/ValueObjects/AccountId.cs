namespace Atm.Application.Models.Accounts.ValueObjects;

public readonly record struct AccountId
{
    public long Value { get; }

    private AccountId(long value)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(value);

        Value = value;
    }

    public static AccountId Create(long value)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(value);

        return new AccountId(value);
    }

    public static AccountId Default()
    {
        return new AccountId(0);
    }
}