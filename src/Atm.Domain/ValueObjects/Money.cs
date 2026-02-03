namespace Atm.Domain.ValueObjects;

public sealed class Money
{
    public Money(decimal value)
    {
        if (value < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(value), value, "Value must be greater than zero.");
        }

        Value = value;
    }

    public decimal Value { get; }

    public Money Add(Amount amount)
    {
        return new Money(Value + amount.Value);
    }

    public Money Subtract(Amount amount)
    {
        if (Value < amount.Value)
        {
            throw new InvalidOperationException("Cannot subtract from non-overflow.");
        }

        return new Money(Value - amount.Value);
    }
}