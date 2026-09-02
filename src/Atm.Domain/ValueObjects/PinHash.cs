namespace Atm.Domain.ValueObjects;

public sealed record PinHash
{
    public PinHash(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length > 512)
            throw new ArgumentException("PIN hash must contain between 1 and 512 characters.", nameof(value));

        Value = value;
    }

    public string Value { get; }

    public override string ToString() => "[REDACTED]";
}
