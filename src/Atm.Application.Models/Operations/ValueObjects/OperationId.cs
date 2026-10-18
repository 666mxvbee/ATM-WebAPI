namespace Atm.Application.Models.Operations.ValueObjects;

public readonly record struct OperationId
{
    public long Value { get; }

    private OperationId(long value)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(value);

        Value = value;
    }

    public static OperationId Create(long value)
    {
        return new OperationId(value);
    }

    public static OperationId Default()
    {
        return new OperationId(0);
    }
}