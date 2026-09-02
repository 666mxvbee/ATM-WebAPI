namespace Atm.Domain.ValueObjects;

public sealed class UserLogin
{
    public UserLogin(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Login is required.", nameof(value));

        Value = value.Trim();
        NormalizedValue = Value.ToUpperInvariant();

        if (Value.Length > 64 || NormalizedValue.Length > 64)
            throw new ArgumentException("Login cannot exceed 64 characters.", nameof(value));
    }

    public string Value { get; }

    public string NormalizedValue { get; }
}
