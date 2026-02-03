using Atm.Application.Abstractions.Repositories;
using Atm.Application.Contracts.Sessions.CreateUserSession;
using Atm.Domain.Sessions;
using Atm.Domain.ValueObjects;

namespace Atm.Application.Sessions;

internal sealed class CreateUserSessionUseCase : ICreateUserSessionPort
{
    private readonly ISessionRepository _sessions;
    private readonly IAccountRepository _accounts;

    public CreateUserSessionUseCase(ISessionRepository sessions, IAccountRepository accounts)
    {
        _sessions = sessions;
        _accounts = accounts;
    }

    public CreateUserSessionResult Execute(CreateUserSessionRequest request)
    {
        try
        {
            var number = new AccountNumber(request.AccountNumber);
            var pin = new PinCode(request.Pin);
            Domain.Accounts.Account? acc = _accounts.GetByNumber(number);

            if (acc is null || acc.Pin.Value != pin.Value)
            {
                return new CreateUserSessionResult.Unauthorized("Invalid credentials");
            }

            var id = Guid.NewGuid();
            var session = new Session(id, SessionType.User, number);

            _sessions.Add(session);

            return new CreateUserSessionResult.Success(id);
        }
        catch (Exception ex)
        {
            return new CreateUserSessionResult.Failure(ex.Message);
        }
    }
}