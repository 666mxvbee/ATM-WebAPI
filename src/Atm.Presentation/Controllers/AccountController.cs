using Atm.Application.Contracts.Accounts;
using Atm.Application.Contracts.Accounts.Models;
using Atm.Application.Contracts.Accounts.Operations;
using Atm.Presentation.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Atm.Presentation.Controllers;

[ApiController]
[Route("api/accounts")]
public sealed class AccountController : ControllerBase
{
    private readonly IAccountService _accountService;

    public AccountController(IAccountService accountService)
    {
        _accountService = accountService;
    }

    [HttpGet("balance")]
    public async Task<ActionResult<BalanceDto>> GetAccountBalanceAsync(
        [FromBody] BalanceRequest httpRequest,
        CancellationToken cancellationToken)
    {
        var request = new GetBalance.Request(
            httpRequest.SessionId);

        GetBalance.Response response = await _accountService
            .GetAccountBalanceAsync(request, cancellationToken);

        return response switch
        {
            GetBalance.Response.Success success => Ok(success.Balance),
            GetBalance.Response.Failure failure => BadRequest(failure.ErrorMessage),
            _ => throw new UnreachableException(),
        };
    }

    [HttpPost("withdraw")]
    public async Task<ActionResult<BalanceDto>> WithdrawBalanceAsync(
        [FromBody] WithdrawRequest httpRequest,
        CancellationToken cancellationToken)
    {
        var request = new WithdrawBalance.Request(
            httpRequest.SessionId,
            httpRequest.Money);

        WithdrawBalance.Response response = await _accountService
            .WithdrawBalanceAsync(request, cancellationToken);

        return response switch
        {
            WithdrawBalance.Response.Success success => Ok(success.Balance),
            WithdrawBalance.Response.Failure failure => BadRequest(failure.ErrorMessage),
            _ => throw new UnreachableException(),
        };
    }

    [HttpPost("deposit")]
    public async Task<ActionResult<BalanceDto>> DepositBalanceAsync(
        [FromBody] DepositRequest httpRequest,
        CancellationToken cancellationToken)
    {
        var request = new DepositBalance.Request(
            httpRequest.SessionId,
            httpRequest.Money);

        DepositBalance.Response response = await _accountService
            .DepositBalanceAsync(request, cancellationToken);

        return response switch
        {
            DepositBalance.Response.Success success => Ok(success.Balance),
            DepositBalance.Response.Failure failure => BadRequest(failure.ErrorMessage),
            _ => throw new UnreachableException(),
        };
    }

    [HttpGet("history")]
    public async Task<ActionResult<OperationHistoryDto>> GetOperationHistoryAsync(
        [FromQuery] OperationHistoryRequest httpRequest,
        CancellationToken cancellationToken)
    {
        var request = new GetOperationHistory.Request(
            httpRequest.SessionId);

        GetOperationHistory.Response response = await _accountService
            .GetOperationHistoryAsync(request, cancellationToken);

        return response switch
        {
            GetOperationHistory.Response.Success success => Ok(success.OperationHistory),
            GetOperationHistory.Response.Failure failure => BadRequest(failure.ErrorMessage),
            _ => throw new UnreachableException(),
        };
    }
}