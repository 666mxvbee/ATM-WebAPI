using Atm.Application.Abstractions.Repositories;
using Atm.Application.Abstractions.Time;
using Atm.Application.Contracts.Sessions.CreateAdminSession;
using Atm.Domain.Sessions;
using Atm.Domain.Sessions.Results;
using Atm.Domain.ValueObjects;
using Microsoft.Extensions.Options;

namespace Atm.Application.Sessions;

internal sealed class CreateAdminSessionUseCase : ICreateAdminSessionPort
{
    private readonly ISessionRepository _sessions;
    private readonly ISystemPasswordProvider _passwords;
    private readonly IClock _clock;
    private readonly SessionOptions _options;

    public CreateAdminSessionUseCase(
        ISessionRepository sessions,
        ISystemPasswordProvider passwords,
        IClock clock,
        IOptions<SessionOptions> options)
    {
        _sessions = sessions;
        _passwords = passwords;
        _clock = clock;
        _options = options.Value;
    }

    public CreateAdminSessionResult Execute(CreateAdminSessionRequest request)
    {
        string expected = _passwords.GetSystemPassword();
        if (string.IsNullOrWhiteSpace(expected))
            return new CreateAdminSessionResult.Failure("System password is not configured");
        if (!string.Equals(request.SystemPassword, expected, StringComparison.Ordinal))
            return new CreateAdminSessionResult.Unauthorized("Invalid system password");

        try
        {
            // Kept only for backward compatibility with the current HTTP contract.
            _ = new AccountNumber(request.AccountNumber);
        }
        catch (ArgumentException exception)
        {
            return new CreateAdminSessionResult.Failure(exception.Message);
        }

        DateTimeOffset createdAtUtc = _clock.UtcNow;
        SessionCreationResult result = Session.Create(
            Guid.NewGuid(),
            SessionType.Admin,
            null,
            createdAtUtc,
            createdAtUtc.AddMinutes(_options.AdminLifetimeMinutes));

        if (result is SessionCreationResult.Failure failure)
            return new CreateAdminSessionResult.Failure(failure.ErrorMessage);

        Session session = ((SessionCreationResult.Success)result).Session;
        _sessions.Add(session);
        return new CreateAdminSessionResult.Success(session.Id);
    }
}
