using Atm.Domain.Common;
using Atm.Domain.Sessions.Results;

namespace Atm.Domain.Sessions;

public sealed class Session
{
    private Session() { }

    public Guid Id { get; private set; }

    public SessionType Type { get; private set; }

    public Guid? AccountId { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset ExpiresAtUtc { get; private set; }

    public DateTimeOffset? RevokedAtUtc { get; private set; }

    public static SessionCreationResult Create(
        Guid id,
        SessionType type,
        Guid? accountId,
        DateTimeOffset createdAtUtc,
        DateTimeOffset expiresAtUtc)
    {
        if (id == Guid.Empty)
            return new SessionCreationResult.Failure("Session ID must not be empty.");
        if (!Enum.IsDefined(type))
            return new SessionCreationResult.Failure("Session type is not supported.");
        if (type == SessionType.User && (accountId is null || accountId == Guid.Empty))
            return new SessionCreationResult.Failure("User session must reference an account.");
        if (type == SessionType.Admin && accountId is not null)
            return new SessionCreationResult.Failure("System admin session must not reference an account.");
        if (!DomainRules.IsUtc(createdAtUtc) || !DomainRules.IsUtc(expiresAtUtc))
            return new SessionCreationResult.Failure("Session timestamps must be UTC.");
        if (expiresAtUtc <= createdAtUtc)
            return new SessionCreationResult.Failure("Session expiration must follow its creation time.");

        return new SessionCreationResult.Success(new Session
        {
            Id = id,
            Type = type,
            AccountId = accountId,
            CreatedAtUtc = createdAtUtc,
            ExpiresAtUtc = expiresAtUtc,
        });
    }

    public bool IsActive(DateTimeOffset utcNow)
    {
        return DomainRules.IsUtc(utcNow)
            && utcNow >= CreatedAtUtc
            && utcNow < ExpiresAtUtc
            && RevokedAtUtc is null;
    }

    public SessionRevocationResult Revoke(DateTimeOffset revokedAtUtc)
    {
        if (RevokedAtUtc is DateTimeOffset existing)
            return new SessionRevocationResult.Success(existing);
        if (!DomainRules.IsUtc(revokedAtUtc) || revokedAtUtc < CreatedAtUtc)
            return new SessionRevocationResult.Failure("Revocation time must be UTC and must not precede session creation.");

        RevokedAtUtc = revokedAtUtc;
        return new SessionRevocationResult.Success(revokedAtUtc);
    }
}
