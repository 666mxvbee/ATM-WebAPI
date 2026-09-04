using Atm.Application.Abstractions.Repositories;
using Atm.Application.Common;
using Atm.Domain.Accounts;
using Atm.Domain.Sessions;
using Atm.Domain.Sessions.Results;
using Atm.Domain.ValueObjects;
using NSubstitute;

namespace Atm.Application.Tests;

public sealed class SessionTests
{
    [Fact]
    public void CreateEnforcesIdentityShapeAndUtcLifetime()
    {
        var clock = new FixedClock();

        Assert.IsType<SessionCreationResult.Failure>(Session.Create(
            Guid.Empty, SessionType.Admin, null, clock.UtcNow, clock.UtcNow.AddMinutes(1)));
        Assert.IsType<SessionCreationResult.Failure>(Session.Create(
            Guid.NewGuid(), SessionType.User, null, clock.UtcNow, clock.UtcNow.AddMinutes(1)));
        Assert.IsType<SessionCreationResult.Failure>(Session.Create(
            Guid.NewGuid(), SessionType.Admin, Guid.NewGuid(), clock.UtcNow, clock.UtcNow.AddMinutes(1)));
        Assert.IsType<SessionCreationResult.Failure>(Session.Create(
            Guid.NewGuid(), SessionType.Admin, null, clock.UtcNow, clock.UtcNow));
        Assert.IsType<SessionCreationResult.Failure>(Session.Create(
            Guid.NewGuid(), SessionType.Admin, null,
            clock.UtcNow.ToOffset(TimeSpan.FromHours(3)), clock.UtcNow.AddMinutes(1)));
    }

    [Fact]
    public void IsActiveUsesInclusiveStartAndExclusiveExpiration()
    {
        var clock = new FixedClock();
        Session session = BankingTestData.CreateAdminSession(Guid.NewGuid(), clock);

        Assert.True(session.IsActive(clock.UtcNow));
        Assert.True(session.IsActive(session.ExpiresAtUtc.AddTicks(-1)));
        Assert.False(session.IsActive(session.ExpiresAtUtc));
        Assert.False(session.IsActive(clock.UtcNow.AddTicks(-1)));
        Assert.False(session.IsActive(clock.UtcNow.ToOffset(TimeSpan.FromHours(3))));
    }

    [Fact]
    public void RevokeIsIdempotentAndKeepsTheFirstTimestamp()
    {
        var clock = new FixedClock();
        Session session = BankingTestData.CreateAdminSession(Guid.NewGuid(), clock);
        DateTimeOffset first = clock.UtcNow.AddMinutes(1);

        Assert.IsType<SessionRevocationResult.Failure>(session.Revoke(clock.UtcNow.AddTicks(-1)));
        Assert.IsType<SessionRevocationResult.Success>(session.Revoke(first));
        SessionRevocationResult.Success repeated = Assert.IsType<SessionRevocationResult.Success>(
            session.Revoke(first.AddMinutes(1)));

        Assert.Equal(first, repeated.RevokedAtUtc);
        Assert.Equal(first, session.RevokedAtUtc);
        Assert.False(session.IsActive(first));
    }

    [Fact]
    public void AuthorizerRejectsExpiredAndRevokedSessions()
    {
        ISessionRepository sessions = Substitute.For<ISessionRepository>();
        var clock = new FixedClock();
        Account account = BankingTestData.CreateAccount(new AccountNumber("session-test"), 0m);
        var sessionId = Guid.NewGuid();
        Session session = BankingTestData.CreateUserSession(sessionId, account, clock);
        sessions.GetById(sessionId).Returns(session);
        var authorizer = new SessionAuthorizer(sessions, clock);

        Assert.IsType<AuthorizationResult.UserSuccess>(authorizer.AuthorizeUser(sessionId));

        clock.UtcNow = session.ExpiresAtUtc;
        Assert.IsType<AuthorizationResult.Unauthorized>(authorizer.AuthorizeUser(sessionId));

        clock.UtcNow = session.CreatedAtUtc;
        Assert.IsType<SessionRevocationResult.Success>(session.Revoke(clock.UtcNow));
        Assert.IsType<AuthorizationResult.Unauthorized>(authorizer.AuthorizeUser(sessionId));
    }
}
