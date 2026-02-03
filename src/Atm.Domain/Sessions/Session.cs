using Atm.Domain.ValueObjects;

namespace Atm.Domain.Sessions;

public sealed class Session
{
    public Session(Guid id, SessionType type, AccountNumber? accountNumber)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Session ID cannot be empty");
        }

        if (type == SessionType.User && accountNumber is null)
        {
            throw new ArgumentException("User session must contain account number");
        }

        if (type == SessionType.Admin && accountNumber is null)
        {
            throw new ArgumentException("Admin session must contain account number");
        }

        Id = id;
        Type = type;
        AccountNumber = accountNumber;
    }

    public Guid Id { get; }

    public SessionType Type { get; }

    public AccountNumber? AccountNumber { get; }
}