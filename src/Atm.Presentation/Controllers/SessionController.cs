using Atm.Application.Contracts.Sessions;
using Atm.Application.Contracts.Sessions.Models;
using Atm.Application.Contracts.Sessions.Operations;
using Atm.Presentation.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Atm.Presentation.Controllers;

[ApiController]
[Route("/api/sessions")]
public sealed class SessionController : ControllerBase
{
    private readonly ISessionService _sessionService;

    public SessionController(ISessionService sessionService)
    {
        _sessionService = sessionService;
    }

    [HttpPost("user")]
    public async Task<ActionResult<SessionDto>> CreateUserSessionAsync(
        [FromBody] CreateUserSessionRequest httpRequest,
        CancellationToken cancellationToken)
    {
        var request = new CreateUserSession.Request(
            httpRequest.AccountNumber,
            httpRequest.PinCode);

        CreateUserSession.Response response = await _sessionService
            .CreateUserSessionAsync(
                request,
                cancellationToken);

        return response switch
        {
            CreateUserSession.Response.Success success => Ok(success.Session),
            CreateUserSession.Response.Failure failure => Unauthorized(failure.ErrorMessage),
            _ => throw new UnreachableException(),
        };
    }

    [HttpPost("admin")]
    public async Task<ActionResult<SessionDto>> CreateAdminSessionAsync(
        [FromBody] CreateAdminSessionRequest httpRequest,
        CancellationToken cancellationToken)
    {
        var request = new CreateAdminSession.Request(
            httpRequest.SystemPassword);

        CreateAdminSession.Response response = await _sessionService
            .CreateAdminSessionAsync(
                request,
                cancellationToken);

        return response switch
        {
            CreateAdminSession.Response.Success success => Ok(success.Session),
            CreateAdminSession.Response.Failure failure => Unauthorized(failure.ErrorMessage),
            _ => throw new UnreachableException(),
        };
    }
}