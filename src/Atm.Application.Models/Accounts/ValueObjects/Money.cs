namespace Atm.Application.Models.Accounts.ValueObjects;

public readonly record struct Money
{
    public decimal Value { get; }

    public static Money Zero => Create(0);

    private Money(decimal value)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(value);

        Value = value;
    }

    public static Money Create(decimal value)
    {
        return new Money(value);
    }

    public static Money operator +(Money left, Money right) => Create(left.Value + right.Value);

    public static Money operator -(Money left, Money right) => Create(left.Value - right.Value);

    public static bool operator >(Money left, Money right) => left.Value > right.Value;

    public static bool operator <(Money left, Money right) => left.Value < right.Value;
}