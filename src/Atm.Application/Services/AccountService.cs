using Atm.Application.Abstractions.Persistence;
using Atm.Application.Contracts.Accounts;
using Atm.Application.Contracts.Accounts.Operations;
using Atm.Application.Models.Accounts;
using Atm.Application.Models.Accounts.Results;
using Atm.Application.Models.Accounts.ValueObjects;
using Atm.Application.Models.Operations;
using Atm.Application.Models.Sessions;
using Atm.HighLoad.Application.Mapping;

namespace Atm.HighLoad.Application.Services;

public sealed class AccountService : IAccountService
{
    private readonly IPersistenceContext _context;

    public AccountService(IPersistenceContext context)
    {
        _context = context;
    }

    public async Task<GetBalance.Response> GetAccountBalanceAsync(
        GetBalance.Request request,
        CancellationToken cancellationToken)
    {
        UserSession? session = await _context.UserSessionRepository
            .FindUserSessionByIdAsync(
                request.SessionId,
                cancellationToken);

        if (session is null)
        {
            return new GetBalance.Response.Failure("UserSession does not exist.");
        }

        Account? account = await _context.AccountRepository
            .FindAccountByIdAsync(
                session.AccountId,
                cancellationToken);

        if (account is null)
        {
            return new GetBalance.Response.Failure("Account does not exist.");
        }

        var operation = Operation.CreateFromService(
            account.Id,
            DateTimeOffset.Now,
            OperationType.ShowBalance,
            Money.Zero,
            account.Balance);

        await _context.OperationHistoryRepository
            .AddOperationAsync(
                operation,
                cancellationToken);

        return new GetBalance.Response.Success(account.Balance.BalanceToDto());
    }

    public async Task<WithdrawBalance.Response> WithdrawBalanceAsync(
        WithdrawBalance.Request request,
        CancellationToken cancellationToken)
    {
        UserSession? session = await _context.UserSessionRepository
            .FindUserSessionByIdAsync(
                request.SessionId,
                cancellationToken);

        if (session is null)
        {
            return new WithdrawBalance.Response.Failure("UserSession does not exist.");
        }

        Account? account = await _context.AccountRepository
            .FindAccountByIdAsync(
                session.AccountId,
                cancellationToken);

        if (account is null)
        {
            return new WithdrawBalance.Response.Failure("Account does not exist.");
        }

        if (request.Money <= 0)
        {
            return new WithdrawBalance.Response.Failure("Invalid money.");
        }

        var amount = Money.Create(request.Money);

        WithdrawResult result = account.Withdraw(amount);

        if (result is WithdrawResult.Failure failure)
        {
            return new WithdrawBalance.Response.Failure(failure.ErrorMessage);
        }

        await _context.AccountRepository
            .UpdateAccountAsync(
                account,
                cancellationToken);

        var operation = Operation.CreateFromService(
            account.Id,
            DateTimeOffset.Now,
            OperationType.Withdraw,
            amount,
            account.Balance);

        await _context.OperationHistoryRepository
           .AddOperationAsync(
               operation,
               cancellationToken);

        return new WithdrawBalance.Response.Success(account.Balance.BalanceToDto());
    }

    public async Task<DepositBalance.Response> DepositBalanceAsync(
        WithdrawBalance.Request request,
        CancellationToken cancellationToken)
    {
        UserSession? session = await _context.UserSessionRepository
            .FindUserSessionByIdAsync(
                request.SessionId,
                cancellationToken);

        if (session is null)
        {
            return new DepositBalance.Response.Failure("UserSession does not exist.");
        }

        Account? account = await _context.AccountRepository
            .FindAccountByIdAsync(
                session.AccountId,
                cancellationToken);

        if (account is null)
        {
            return new DepositBalance.Response.Failure("Account does not exist.");
        }

        var amount = Money.Create(request.Money);

        account.Deposit(amount);

        await _context.AccountRepository.UpdateAccountAsync(
            account,
            cancellationToken);

        var operation = Operation.CreateFromService(
            account.Id,
            DateTimeOffset.Now,
            OperationType.Deposit,
            amount,
            account.Balance);

        await _context.OperationHistoryRepository
            .AddOperationAsync(
                operation,
                cancellationToken);

        return new DepositBalance.Response.Success(account.Balance.BalanceToDto());
    }

    public async Task<GetOperationHistory.Response> GetOperationHistoryAsync(
        GetOperationHistory.Request request,
        CancellationToken cancellationToken)
    {
        UserSession? session = await _context.UserSessionRepository
            .FindUserSessionByIdAsync(
                request.SessionId,
                cancellationToken);

        if (session is null)
        {
            return new GetOperationHistory.Response.Failure("UserSession does not exist.");
        }

        Account? account = await _context.AccountRepository
            .FindAccountByIdAsync(
                session.AccountId,
                cancellationToken);

        if (account is null)
        {
            return new GetOperationHistory.Response.Failure("Account does not exist.");
        }

        var operations = new List<Operation>();

        await foreach (Operation operation in _context.OperationHistoryRepository
                           .FindByAccountIdAsync(account.Id, cancellationToken)
                           .WithCancellation(cancellationToken))
        {
            operations.Add(operation);
        }

        return new GetOperationHistory.Response.Success(operations.HistoryToDto());
    }
}