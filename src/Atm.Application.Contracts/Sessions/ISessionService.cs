using Atm.Application.Contracts.Sessions.Operations;

namespace Atm.Application.Contracts.Sessions;

public interface ISessionService
{
    Task<CreateUserSession.Response> CreateUserSessionAsync(
        CreateUserSession.Request request,
        CancellationToken cancellationToken);

    Task<CreateAdminSession.Response> CreateAdminSessionAsync(
        CreateAdminSession.Request request,
        CancellationToken cancellationToken);
}