using Atm.Application.Contracts.Sessions.Models;
using Atm.Application.Models.Sessions;

namespace Atm.HighLoad.Application.Mapping;

public static class SessionMappingExtensions
{
    public static SessionDto UserSessionMapToDto(
        this UserSession userSession)
    {
        return new SessionDto(userSession.SessionId);
    }

    public static SessionDto AdminSessionMapToDto(
        this AdminSession adminSession)
    {
        return new SessionDto(adminSession.SessionId);
    }
}