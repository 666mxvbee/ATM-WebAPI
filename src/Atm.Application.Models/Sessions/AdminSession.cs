namespace Atm.Application.Models.Sessions;

public sealed class AdminSession
{
    public Guid SessionId { get; }

    public AdminSession(Guid sessionId)
    {
        SessionId = sessionId;
    }
}