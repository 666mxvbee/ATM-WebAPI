using Atm.Application.Abstractions.Repositories;
using Atm.Application.Contracts.Sessions.CreateAdminSession;
using Atm.Domain.Sessions;
using Atm.Domain.ValueObjects;

namespace Atm.Application.Sessions;

internal sealed class CreateAdminSessionUseCase : ICreateAdminSessionPort
{
    private readonly ISessionRepository _sessions;
    private readonly ISystemPasswordProvider _passwords;

    public CreateAdminSessionUseCase(ISessionRepository sessions, ISystemPasswordProvider passwords)
    {
        _sessions = sessions;
        _passwords = passwords;
    }

    public CreateAdminSessionResult Execute(CreateAdminSessionRequest request)
    {
        string expected = _passwords.GetSystemPassword();

        if (string.IsNullOrWhiteSpace(expected))
        {
            return new CreateAdminSessionResult.Failure("System password is not configured");
        }

        if (request.SystemPassword != expected)
        {
            return new CreateAdminSessionResult.Unauthorized("Invalid system password");
        }

        try
        {
            var accountNumber = new AccountNumber(request.AccountNumber);
            var id = Guid.NewGuid();

            _sessions.Add(new Session(id, SessionType.Admin, accountNumber));

            return new CreateAdminSessionResult.Success(id);
        }
        catch(Exception ex)
        {
            return new CreateAdminSessionResult.Failure(ex.Message);
        }
    }
}