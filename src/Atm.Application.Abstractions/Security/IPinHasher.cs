using Atm.Domain.ValueObjects;

namespace Atm.Application.Abstractions.Security;

public interface IPinHasher
{
    PinHash Hash(PinCode pin);

    bool Verify(PinHash hash, PinCode pin);
}
