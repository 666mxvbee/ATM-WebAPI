using Atm.Application.Contracts.Accounts.CreateAccount;
using Atm.Application.Contracts.Accounts.GetBalance;
using Atm.Application.Contracts.Operations.Deposit;
using Atm.Application.Contracts.Operations.GetTransactionHistory;
using Atm.Application.Contracts.Operations.Withdraw;
using Microsoft.AspNetCore.Mvc;

namespace Atm.Presentation.Controllers;

[ApiController]
[Route("api/accounts")]
public sealed class AccountsController : ControllerBase
{
    private readonly ICreateAccountPort _createAccount;
    private readonly IGetBalancePort _getBalance;
    private readonly IDepositPort _deposit;
    private readonly IWithdrawPort _withdraw;
    private readonly IGetTransactionHistoryPort _history;

    public AccountsController(
        ICreateAccountPort createAccount,
        IGetBalancePort getBalance,
        IDepositPort deposit,
        IWithdrawPort withdraw,
        IGetTransactionHistoryPort history)
    {
        _createAccount = createAccount;
        _getBalance = getBalance;
        _deposit = deposit;
        _withdraw = withdraw;
        _history = history;
    }

    [HttpPost("create")]
    public IActionResult Create([FromBody] CreateAccountApiRequest request)
    {
        CreateAccountResult result = _createAccount.Execute(new CreateAccountRequest(request.AdminSessionId, request.AccountNumber, request.Pin, request.InitialBalance));

        return result switch
        {
            CreateAccountResult.Success => Ok(new { message = "Account created successfully" }),
            CreateAccountResult.Unauthorized u => StatusCode(401, new { error = u.ErrorMessage }),
            CreateAccountResult.Failure f => BadRequest(new { error = f.ErrorMessage }),
            _ => BadRequest(new { error = "Unknown result" }),
        };
    }

    [HttpGet("balance")]
    public IActionResult Balance([FromQuery(Name = "sessionId")] Guid userSessionId)
    {
        GetBalanceResult result = _getBalance.Execute(new GetBalanceRequest(userSessionId));

        if (result is GetBalanceResult.Success s)
        {
            return Ok(new { balance = s.Balance });
        }

        if (result is GetBalanceResult.Unauthorized u)
        {
            return StatusCode(401, new { error = u.ErrorMessage });
        }

        if (result is GetBalanceResult.Failure f)
        {
            return BadRequest(new { error = f.ErrorMessage });
        }

        return BadRequest(new { error = "Unknown result" });
    }

    [HttpPost("deposit")]
    public IActionResult Deposit([FromBody] AmountApiRequest request)
    {
        DepositResult result = _deposit.Execute(new DepositRequest(request.UserSessionId, request.Amount));

        if (result is DepositResult.Success s)
        {
            return Ok(new { balance = s.Balance });
        }

        if (result is DepositResult.Unauthorized u)
        {
            return StatusCode(401, new { error = u.ErrorMessage });
        }

        if (result is DepositResult.Failure f)
        {
            return BadRequest(new { error = f.ErrorMessage });
        }

        return BadRequest(new { error = "Unknown result" });
    }

    [HttpPost("withdraw")]
    public IActionResult Withdraw([FromBody] AmountApiRequest request)
    {
        WithdrawResult result = _withdraw.Execute(new WithdrawRequest(request.UserSessionId, request.Amount));

        if (result is WithdrawResult.Success s)
        {
            return Ok(new { balance = s.Balance });
        }

        if (result is WithdrawResult.Unauthorized u)
        {
            return StatusCode(401, new { error = u.ErrorMessage });
        }

        if (result is WithdrawResult.Failure f)
        {
            return BadRequest(new { error = f.ErrorMessage });
        }

        return BadRequest(new { error = "Unknown result" });
    }

    [HttpGet("history")]
    public IActionResult History([FromQuery] Guid userSessionId)
    {
        GetTransactionHistoryResult result = _history.Execute(new GetTransactionHistoryRequest(userSessionId));

        if (result is GetTransactionHistoryResult.Success s)
        {
            return Ok(new { transactions = s.Transactions });
        }

        if (result is GetTransactionHistoryResult.Unauthorized u)
        {
            return StatusCode(401, new { error = u.ErrorMessage });
        }

        if (result is GetTransactionHistoryResult.Failure f)
        {
            return BadRequest(new { error = f.ErrorMessage });
        }

        return BadRequest(new { error = "Unknown result" });
    }

    public sealed record CreateAccountApiRequest(Guid AdminSessionId, string AccountNumber, string Pin, decimal InitialBalance);

    public sealed record AmountApiRequest(Guid UserSessionId, decimal Amount);
}