namespace Atm.Domain.ValueObjects;

public sealed class AccountNumber
{
    public AccountNumber(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            throw new ArgumentException("Value cannot be null or empty.", nameof(value));
        }

        Value = value.Trim();
    }

    public string Value { get; }
}