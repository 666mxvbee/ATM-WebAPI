namespace Atm.Domain.ValueObjects;

public sealed class Amount
{
    public Amount(decimal value)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(value);

        Value = value;
    }

    public decimal Value { get; }
}