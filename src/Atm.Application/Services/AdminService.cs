using Atm.Application.Abstractions;
using Atm.Application.Contracts.Admins;
using Atm.Application.Contracts.Admins.Models;
using Atm.Application.Models.Accounts;
using Atm.Application.Models.Accounts.ValueObjects;
using Atm.Application.Models.Sessions;
using Atm.Application.Mapping;

namespace Atm.Application.Services;

public sealed class AdminService : IAdminService
{
    private readonly IPersistenceContext _context;

    public AdminService(IPersistenceContext context)
    {
        _context = context;
    }

    public async Task<CreateAccount.Response> CreateAccountAsync(
        CreateAccount.Request request,
        CancellationToken cancellationToken)
    {
        AdminSession? adminSession = await _context.AdminSessionRepository
            .FindAdminSessionByIdAsync(request.SessionId, cancellationToken);

        if (adminSession is null)
        {
            return new CreateAccount.Response.Failure("Admin session was not found.");
        }

        var accountNumber = AccountNumber.Create(request.AccountNumber);
        var pincode = Pincode.Create(request.PinCode);

        Account? account = await _context.AccountRepository
                .FindAccountByNumberAsync(accountNumber, cancellationToken);

        if (account is not null)
        {
            return new CreateAccount.Response.Failure("Account with this AccountNumber already exists.");
        }

        var newAccount = Account.CreateFromService(
            accountNumber,
            pincode);

        Account createdAccount = await _context.AccountRepository
            .AddAccountAsync(newAccount, cancellationToken);

        return new CreateAccount.Response.Success(createdAccount.AccountMapToDto());
    }
}