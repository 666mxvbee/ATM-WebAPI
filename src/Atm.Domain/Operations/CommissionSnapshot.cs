using Atm.Domain.Common;
using Atm.Domain.ValueObjects;

namespace Atm.Domain.Operations;

public sealed class CommissionSnapshot
{
    private CommissionSnapshot() { }

    public CommissionSnapshot(Amount operationAmount, decimal rate, decimal fixedAmount)
    {
        ArgumentNullException.ThrowIfNull(operationAmount);
        if (rate < 0m || rate > 1m || !DomainRules.HasScale(rate, 6))
            throw new ArgumentException("Commission rate must be between zero and one with at most six decimal places.", nameof(rate));

        var fixedMoney = new Money(fixedAmount);
        decimal proportional = checked(operationAmount.Value * rate);
        decimal unrounded = DomainRules.AddExact(proportional, fixedMoney.Value);
        decimal rounded = decimal.Round(unrounded, 4, MidpointRounding.ToEven);

        Rate = rate;
        FixedAmount = fixedMoney;
        CommissionAmount = new Money(rounded);
    }

    public decimal Rate { get; private set; }

    public Money FixedAmount { get; private set; } = null!;

    public Money CommissionAmount { get; private set; } = null!;

    public bool IsCalculatedFor(Amount operationAmount)
    {
        ArgumentNullException.ThrowIfNull(operationAmount);
        try
        {
            var expected = new CommissionSnapshot(operationAmount, Rate, FixedAmount.Value);
            return expected.CommissionAmount.Value == CommissionAmount.Value;
        }
        catch (OverflowException)
        {
            return false;
        }
    }
}
