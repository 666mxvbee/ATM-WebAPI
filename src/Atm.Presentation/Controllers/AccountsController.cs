using Atm.Application.Contracts.Accounts.CreateAccount;
using Atm.Application.Contracts.Accounts.GetBalance;
using Atm.Application.Contracts.Operations.Deposit;
using Atm.Application.Contracts.Operations.GetTransactionHistory;
using Atm.Application.Contracts.Operations.Withdraw;
using Atm.Presentation.Contracts;
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
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public IActionResult Create([FromBody] CreateAccountApiRequest request)
    {
        CreateAccountResult result = _createAccount.Execute(
            new CreateAccountRequest(
                request.AdminSessionId,
                request.AccountNumber,
                request.Pin,
                request.InitialBalance));

        return result switch
        {
            CreateAccountResult.Success => Ok(new MessageResponse("Account created")),
            CreateAccountResult.Unauthorized u => Unauthorized(new ErrorResponse(u.ErrorMessage)),
            CreateAccountResult.Failure f => BadRequest(new ErrorResponse(f.ErrorMessage)),
            _ => BadRequest(new ErrorResponse("Unknown error")),
        };
    }

    [HttpGet("balance")]
    [ProducesResponseType(typeof(BalanceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public IActionResult Balance([FromQuery(Name = "sessionId")] Guid userSessionId)
    {
        GetBalanceResult result = _getBalance.Execute(
            new GetBalanceRequest(userSessionId));
        
        return result switch
        {
            GetBalanceResult.Success s => Ok(new BalanceResponse(s.Balance)),
            GetBalanceResult.Unauthorized u => Unauthorized(new ErrorResponse(u.ErrorMessage)),
            GetBalanceResult.Failure f => BadRequest(new ErrorResponse(f.ErrorMessage)),
            _ => BadRequest(new ErrorResponse("Unknown error")),
        };
    }

    [HttpPost("deposit")]
    [ProducesResponseType(typeof(BalanceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public IActionResult Deposit([FromBody] AmountApiRequest request)
    {
        DepositResult result = _deposit.Execute(
            new DepositRequest(request.UserSessionId, request.Amount));

        return result switch
        {
            DepositResult.Success s => Ok(new BalanceResponse(s.Balance)),
            DepositResult.Unauthorized u => Unauthorized(new ErrorResponse(u.ErrorMessage)),
            DepositResult.Failure f => BadRequest(new ErrorResponse(f.ErrorMessage)),
            _ => BadRequest(new ErrorResponse("Unknown error")),
        };
    }

    [HttpPost("withdraw")]
    [ProducesResponseType(typeof(BalanceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public IActionResult Withdraw([FromBody] AmountApiRequest request)
    {
        WithdrawResult result = _withdraw.Execute(
            new WithdrawRequest(request.UserSessionId, request.Amount));

        return result switch
        {
            WithdrawResult.Success s => Ok(new BalanceResponse(s.Balance)),
            WithdrawResult.Unauthorized u => Unauthorized(new ErrorResponse(u.ErrorMessage)),
            WithdrawResult.Failure f => BadRequest(new ErrorResponse(f.ErrorMessage)),
            _ => BadRequest(new ErrorResponse("Unknown error")),
        };
    }

    [HttpGet("history")]
    [ProducesResponseType(typeof(TransactionsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public IActionResult History([FromQuery(Name = "sessionId")] Guid userSessionId)
    {
        GetTransactionHistoryResult result = _history.Execute(
            new GetTransactionHistoryRequest(userSessionId));
        
        return result switch
        {
            GetTransactionHistoryResult.Success s => Ok(new TransactionsResponse(s.Transactions)),
            GetTransactionHistoryResult.Unauthorized u => Unauthorized(new ErrorResponse(u.ErrorMessage)),
            GetTransactionHistoryResult.Failure f => BadRequest(new ErrorResponse(f.ErrorMessage)),
            _ => BadRequest(new ErrorResponse("Unknown error")),
        };
    }
}
