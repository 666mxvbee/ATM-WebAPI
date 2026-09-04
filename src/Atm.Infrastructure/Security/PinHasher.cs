using Atm.Application.Abstractions.Security;
using Atm.Domain.ValueObjects;
using Microsoft.AspNetCore.Identity;

namespace Atm.Infrastructure.Security;

public sealed class PinHasher : IPinHasher
{
    private readonly PasswordHasher<object> _hasher = new();

    public PinHash Hash(PinCode pin)
    {
        return new PinHash(_hasher.HashPassword(this, pin.Value));
    }

    public bool Verify(PinHash hash, PinCode pin)
    {
        try
        {
            return _hasher.VerifyHashedPassword(this, hash.Value, pin.Value)
                != PasswordVerificationResult.Failed;
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
