using Atm.Application.Abstractions.Repositories;
using Atm.Application.Abstractions.Security;
using Atm.Application.Abstractions.Time;
using Atm.Application.Contracts.Sessions.CreateUserSession;
using Atm.Domain.Sessions;
using Atm.Domain.Sessions.Results;
using Atm.Domain.ValueObjects;
using Microsoft.Extensions.Options;

namespace Atm.Application.Sessions;

internal sealed class CreateUserSessionUseCase : ICreateUserSessionPort
{
    private readonly ISessionRepository _sessions;
    private readonly IAccountRepository _accounts;
    private readonly IPinHasher _pinHasher;
    private readonly IClock _clock;
    private readonly SessionOptions _options;

    public CreateUserSessionUseCase(
        ISessionRepository sessions,
        IAccountRepository accounts,
        IPinHasher pinHasher,
        IClock clock,
        IOptions<SessionOptions> options)
    {
        _sessions = sessions;
        _accounts = accounts;
        _pinHasher = pinHasher;
        _clock = clock;
        _options = options.Value;
    }

    public CreateUserSessionResult Execute(CreateUserSessionRequest request)
    {
        AccountNumber number;
        PinCode pin;
        try
        {
            number = new AccountNumber(request.AccountNumber);
            pin = new PinCode(request.Pin);
        }
        catch (ArgumentException exception)
        {
            return new CreateUserSessionResult.Failure(exception.Message);
        }

        Domain.Accounts.Account? account = _accounts.GetByNumber(number);
        if (account is null || !_pinHasher.Verify(account.PinHash, pin))
            return new CreateUserSessionResult.Unauthorized("Invalid credentials");

        DateTimeOffset createdAtUtc = _clock.UtcNow;
        SessionCreationResult result = Session.Create(
            Guid.NewGuid(),
            SessionType.User,
            account.Id,
            createdAtUtc,
            createdAtUtc.AddMinutes(_options.UserLifetimeMinutes));

        if (result is SessionCreationResult.Failure failure)
            return new CreateUserSessionResult.Failure(failure.ErrorMessage);

        Session session = ((SessionCreationResult.Success)result).Session;
        _sessions.Add(session);
        return new CreateUserSessionResult.Success(session.Id);
    }
}
