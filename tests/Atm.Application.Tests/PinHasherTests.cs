using Atm.Domain.ValueObjects;
using Atm.Infrastructure.Security;

namespace Atm.Application.Tests;

public class PinHasherTests
{
    [Fact]
    public void HashUsesDifferentSaltsAndVerifiesThePin()
    {
        var hasher = new PinHasher();
        var pin = new PinCode("1234");

        PinHash first = hasher.Hash(pin);
        PinHash second = hasher.Hash(pin);

        Assert.NotEqual(pin.Value, first.Value);
        Assert.NotEqual(first.Value, second.Value);
        Assert.True(hasher.Verify(first, pin));
        Assert.True(hasher.Verify(second, pin));
        Assert.False(hasher.Verify(first, new PinCode("4321")));
        Assert.Equal("[REDACTED]", first.ToString());
    }

    [Theory]
    [InlineData("not-base64")]
    [InlineData("AA==")]
    public void VerifyInvalidStoredHashReturnsFalse(string value)
    {
        var hasher = new PinHasher();

        Assert.False(hasher.Verify(new PinHash(value), new PinCode("1234")));
    }
}
