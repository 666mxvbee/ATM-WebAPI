using Atm.Domain.Common;

namespace Atm.Domain.ValueObjects;

public sealed class Amount
{
    public Amount(decimal value)
    {
        if (value <= 0m || !DomainRules.HasScale(value, 4))
            throw new ArgumentException("Amount must be positive with at most four decimal places.", nameof(value));

        Value = value;
    }

    public decimal Value { get; }
}
