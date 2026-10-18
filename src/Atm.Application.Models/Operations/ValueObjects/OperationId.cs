namespace Atm.Application.Models.Operations.ValueObjects;

public readonly record struct OperationId
{
    public long Value { get; }

    private OperationId(long value)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(value);

        Value = value;
    }

    public static OperationId Create(long value)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(value);

        return new OperationId(value);
    }

    public static OperationId Default()
    {
        return new OperationId(0);
    }
}