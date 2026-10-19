using Atm.Application.Contracts.Admins;
using Atm.Application.Contracts.Admins.Models;
using Atm.Application.Contracts.Admins.Operations;
using Atm.Presentation.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Atm.Presentation.Controllers;

[ApiController]
[Route("/api/admin")]
public sealed class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;

    public AdminController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    [HttpPost("accounts")]
    public async Task<ActionResult<AccountDto>> CreateAccountAsync(
        CreateAccountRequest httpRequest,
        CancellationToken cancellationToken)
    {
        var request = new CreateAccount.Request(
            httpRequest.SessionId,
            httpRequest.AccountNumber,
            httpRequest.PinCode);

        CreateAccount.Response response = await _adminService.CreateAccountAsync(request, cancellationToken);
        return response switch
        {
            CreateAccount.Response.Success success => Ok(success.Account),
            CreateAccount.Response.Failure failure => BadRequest(failure.ErrorMessage),
            _ => throw new UnreachableException(),
        };
    }
}