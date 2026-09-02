namespace Atm.Domain.Common;

internal static class DomainRules
{
    public static bool IsUtc(DateTimeOffset value)
    {
        return value.Offset == TimeSpan.Zero;
    }

    public static bool HasScale(decimal value, int scale)
    {
        return decimal.Round(value, scale) == value;
    }

    public static decimal AddExact(decimal left, decimal right)
    {
        decimal result = checked(left + right);
        if (result - left != right || result - right != left)
            throw new OverflowException("The amount cannot be represented exactly as decimal.");
        return result;
    }

    public static decimal SubtractExact(decimal left, decimal right)
    {
        decimal result = checked(left - right);
        if (left - result != right || result + right != left)
            throw new OverflowException("The amount cannot be represented exactly as decimal.");
        return result;
    }
}
