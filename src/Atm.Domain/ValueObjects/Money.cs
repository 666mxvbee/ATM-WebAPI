using Atm.Domain.Common;

namespace Atm.Domain.ValueObjects;

public sealed class Money
{
    public Money(decimal value)
    {
        if (value < 0m || !DomainRules.HasScale(value, 4))
            throw new ArgumentException("Money must be non-negative with at most four decimal places.", nameof(value));

        Value = value;
    }

    public decimal Value { get; }

    public Money Add(Amount amount)
    {
        return new Money(DomainRules.AddExact(Value, amount.Value));
    }

    public Money Subtract(Amount amount)
    {
        if (Value < amount.Value)
            throw new InvalidOperationException("Not enough balance.");
        return new Money(DomainRules.SubtractExact(Value, amount.Value));
    }
}
