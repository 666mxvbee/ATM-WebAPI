namespace Atm.Domain.ValueObjects;

public sealed class PinCode
{
    public PinCode(string value)
    {
        if (value.Length != 4)
        {
            throw new ArgumentOutOfRangeException(nameof(value));
        }

        if (value.Any(c => !char.IsDigit(c)))
        {
            throw new ArgumentException("Pin code must contain only digits");
        }

        Value = value;
    }

    public string Value { get; }
}