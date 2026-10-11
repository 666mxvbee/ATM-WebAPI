namespace Atm.Application.Models.Accounts.ValueObjects;

public readonly record struct Pincode
{
    public string Value { get; }

    private Pincode(string value)
    {
        ArgumentException.ThrowIfNullOrEmpty(value);

        Value = value;
    }

    public static Pincode Create(string value)
    {
        return new Pincode(value);
    }
}