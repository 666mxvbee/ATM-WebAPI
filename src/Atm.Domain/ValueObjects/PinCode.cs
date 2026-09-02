namespace Atm.Domain.ValueObjects;

public sealed class PinCode
{
    public PinCode(string value)
    {
        if (value is null || value.Length != 4 || value.Any(character => character is < '0' or > '9'))
            throw new ArgumentException("PIN must contain exactly four ASCII digits.", nameof(value));
        Value = value;
    }

    public string Value { get; }
}
