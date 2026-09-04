using Atm.Application.Abstractions.Repositories;
using Atm.Application.Common;
using Atm.Domain.Accounts;
using Atm.Domain.Sessions;
using Atm.Domain.Sessions.Results;
using Atm.Domain.ValueObjects;
using NSubstitute;

namespace Atm.Application.Tests;

public sealed class SessionAuthorizerTests
{
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
