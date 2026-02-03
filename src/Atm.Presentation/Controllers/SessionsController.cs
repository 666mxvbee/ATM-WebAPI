using Atm.Application.Contracts.Sessions.CreateAdminSession;
using Atm.Application.Contracts.Sessions.CreateUserSession;
using Atm.Domain.ValueObjects;
using Microsoft.AspNetCore.Mvc;

namespace Atm.Presentation.Controllers;

[ApiController]
[Route("api/sessions")]
public sealed class SessionsController : ControllerBase
{
    private readonly ICreateUserSessionPort _createUserSession;
    private readonly ICreateAdminSessionPort _createAdminSession;

    public SessionsController(
        ICreateUserSessionPort createUserSession,
        ICreateAdminSessionPort createAdminSession)
    {
        _createUserSession = createUserSession;
        _createAdminSession = createAdminSession;
    }

    [HttpPost("user")]
    public IActionResult CreateUser([FromBody] CreateUserSessionApiRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.AccountNumber) || string.IsNullOrWhiteSpace(request.Pin))
        {
            return BadRequest(new { error = "AccountNumber and Pin are required" });
        }

        CreateUserSessionResult result = _createUserSession.Execute(new CreateUserSessionRequest(request.AccountNumber, request.Pin));

        if (result is CreateUserSessionResult.Success s)
        {
            return Ok(new { sessionId = s.SessionId });
        }

        if (result is CreateUserSessionResult.Unauthorized u)
        {
            return StatusCode(401, new { error = u.ErrorMessage });
        }

        if (result is CreateUserSessionResult.Failure f)
        {
            return BadRequest(new { error = f.ErrorMessage });
        }

        return BadRequest(new { error = "Unknown result" });
    }

    [HttpPost("admin")]
    public IActionResult CreateAdmin([FromBody] CreateAdminSessionApiRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.SystemPassword))
        {
            return BadRequest(new { error = "SystemPassword is required" });
        }

        var acc = new AccountNumber(request.AccountNumber);

        CreateAdminSessionResult result = _createAdminSession.Execute(new CreateAdminSessionRequest(request.SystemPassword, acc));

        if (result is CreateAdminSessionResult.Success s)
        {
            return Ok(new { sessionId = s.SessionId });
        }

        if (result is CreateAdminSessionResult.Unauthorized u)
        {
            return StatusCode(401, new { error = u.ErrorMessage });
        }

        if (result is CreateAdminSessionResult.Failure f)
        {
            return BadRequest(new { error = f.ErrorMessage });
        }

        return BadRequest(new { error = "Unknown result" });
    }

    public sealed record CreateUserSessionApiRequest(string AccountNumber, string Pin);

    public sealed record CreateAdminSessionApiRequest(string SystemPassword, string AccountNumber);
}