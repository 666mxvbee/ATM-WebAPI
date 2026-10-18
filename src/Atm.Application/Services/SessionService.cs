using Atm.Application.Abstractions;
using Atm.Application.Contracts.Sessions;
using Atm.Application.Contracts.Sessions.Operations;
using Atm.Application.Models.Accounts;
using Atm.Application.Models.Accounts.ValueObjects;
using Atm.Application.Models.Sessions;
using Atm.HighLoad.Application.Mapping;
using Atm.HighLoad.Application.Options;
using Microsoft.Extensions.Options;

namespace Atm.HighLoad.Application.Services;

public sealed class SessionService : ISessionService
{
    private readonly IPersistenceContext _context;
    private readonly string _systemPassword;

    public SessionService(
        IPersistenceContext context,
        IOptions<SessionOptions> options)
    {
        _context = context;
        _systemPassword = options.Value.SystemPassword;
    }

    public async Task<CreateUserSession.Response> CreateUserSessionAsync(
        CreateUserSession.Request request,
        CancellationToken cancellationToken)
    {
        var accountNumber = AccountNumber.Create(request.AccountNumber);
        var pincode = Pincode.Create(request.Pincode);

        Account? account = await _context.AccountRepository
            .FindAccountByNumberAsync(accountNumber, cancellationToken);

        if (account is null)
        {
            return new CreateUserSession.Response.Failure("Account was not found.");
        }

        if (pincode != account.Pincode)
        {
            return new CreateUserSession.Response.Failure("Wrong pincode.");
        }

        var session = new UserSession(Guid.NewGuid(), account.Id);

        await _context.UserSessionRepository.AddSessionAsync(
            session,
            cancellationToken);

        return new CreateUserSession.Response.Success(session.UserSessionMapToDto());
    }

    public async Task<CreateAdminSession.Response> CreateAdminSessionAsync(
        CreateAdminSession.Request request,
        CancellationToken cancellationToken)
    {
        if (request.SystemPassword != _systemPassword)
        {
            return new CreateAdminSession.Response.Failure("Wrong password.");
        }

        var session = new AdminSession(Guid.NewGuid());

        await _context.AdminSessionRepository.AddSessionAsync(
            session,
            cancellationToken);

        return new CreateAdminSession.Response.Success(session.AdminSessionMapToDto());
    }
}