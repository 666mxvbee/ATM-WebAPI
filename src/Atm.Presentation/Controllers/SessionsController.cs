using Atm.Application.Contracts.Sessions.CreateAdminSession;
using Atm.Application.Contracts.Sessions.CreateUserSession;
using Atm.Presentation.Contracts;
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
    [ProducesResponseType(typeof(SessionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public IActionResult CreateUser([FromBody] CreateUserSessionApiRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.AccountNumber) || string.IsNullOrWhiteSpace(request.Pin))
        {
            return BadRequest(new ErrorResponse("AccountNumber and Pin are required"));
        }

        CreateUserSessionResult result = _createUserSession.Execute(
            new CreateUserSessionRequest(request.AccountNumber, request.Pin));

        return result switch
        {
            CreateUserSessionResult.Success s => Ok(new SessionResponse(s.SessionId)),
            CreateUserSessionResult.Unauthorized u => Unauthorized(new ErrorResponse(u.ErrorMessage)),
            CreateUserSessionResult.Failure f => BadRequest(new ErrorResponse(f.ErrorMessage)),
            _ => BadRequest(new ErrorResponse("Unknown error")),
        };
    }

    [HttpPost("admin")]
    [ProducesResponseType(typeof(SessionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public IActionResult CreateAdmin([FromBody] CreateAdminSessionApiRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.SystemPassword) || string.IsNullOrWhiteSpace(request.AccountNumber))
        {
            return BadRequest(new ErrorResponse("SystemPassword and AccountNumber are required"));
        }

        CreateAdminSessionResult result = _createAdminSession.Execute(
            new CreateAdminSessionRequest(request.SystemPassword, request.AccountNumber));

        return result switch
        {
            CreateAdminSessionResult.Success s => Ok(new SessionResponse(s.SessionId)),
            CreateAdminSessionResult.Unauthorized u => Unauthorized(new ErrorResponse(u.ErrorMessage)),
            CreateAdminSessionResult.Failure f => BadRequest(new ErrorResponse(f.ErrorMessage)),
            _ => BadRequest(new ErrorResponse("Unknown result")),
        };
    }
}
