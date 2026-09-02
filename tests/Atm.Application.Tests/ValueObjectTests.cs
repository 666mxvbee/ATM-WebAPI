using Atm.Domain.ValueObjects;
using System.Globalization;

namespace Atm.Application.Tests;

public sealed class ValueObjectTests
{
    [Fact]
    public void NormalizationIsCultureIndependent()
    {
        CultureInfo original = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("tr-TR");
            var number = new AccountNumber("  idi-01  ");
            var login = new UserLogin("  idi  ");
            Assert.Equal("idi-01", number.Value);
            Assert.Equal("IDI-01", number.NormalizedValue);
            Assert.Equal("IDI", login.NormalizedValue);
        }
        finally
        {
            CultureInfo.CurrentCulture = original;
        }
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void EmptyIdentifiersAreRejected(string value)
    {
        Assert.Throws<ArgumentException>(() => new AccountNumber(value));
        Assert.Throws<ArgumentException>(() => new UserLogin(value));
    }

    [Fact]
    public void LongIdentifiersAreRejected()
    {
        string value = new string('a', 65);
        Assert.Throws<ArgumentException>(() => new AccountNumber(value));
        Assert.Throws<ArgumentException>(() => new UserLogin(value));
    }

    [Fact]
    public void MoneyHasNoNumeric19BusinessCeiling()
    {
        var money = new Money(1000000000000000m);
        Assert.Equal(1000000000000001m, money.Add(new Amount(1m)).Value);
        Assert.Equal(decimal.MaxValue, new Money(decimal.MaxValue).Value);
    }

    [Fact]
    public void InvalidAmountsAndPrecisionAreRejected()
    {
        Assert.Throws<ArgumentException>(() => new Amount(0m));
        Assert.Throws<ArgumentException>(() => new Amount(-1m));
        Assert.Throws<ArgumentException>(() => new Amount(1.00001m));
        Assert.Throws<ArgumentException>(() => new Money(-1m));
        Assert.Throws<ArgumentException>(() => new Money(1.00001m));
    }

    [Fact]
    public void ArithmeticDoesNotSilentlyLoseSmallAmounts()
    {
        var money = new Money(decimal.MaxValue);
        Assert.Throws<OverflowException>(() => money.Add(new Amount(1m)));
        Assert.Throws<OverflowException>(() => money.Add(new Amount(0.0001m)));
        Assert.Throws<OverflowException>(() => money.Subtract(new Amount(0.0001m)));
    }

    [Theory]
    [InlineData("123")]
    [InlineData("abcd")]
    [InlineData("１２３４")]
    public void InvalidPinIsRejectedWithoutEchoingInput(string pin)
    {
        ArgumentException exception = Assert.Throws<ArgumentException>(() => new PinCode(pin));
        Assert.DoesNotContain(pin, exception.Message, StringComparison.Ordinal);
    }
}
