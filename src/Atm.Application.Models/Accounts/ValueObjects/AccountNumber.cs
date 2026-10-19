namespace Atm.Application.Models.Accounts.ValueObjects;

public readonly record struct AccountNumber
{
    public string Value { get; }

    private AccountNumber(string value)
    {
        ArgumentException.ThrowIfNullOrEmpty(value);

        Value = value;
    }

    public static AccountNumber Create(string value)
    {
        return new AccountNumber(value);
    }
}