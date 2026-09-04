using Atm.Domain.Sessions;
using Atm.Domain.Sessions.Results;

namespace Atm.Domain.Tests;

public sealed class SessionTests
{
    [Fact]
    public void CreateEnforcesIdentityShapeAndUtcLifetime()
    {
        DateTimeOffset now = DomainTestData.UtcNow;

        Assert.IsType<SessionCreationResult.Failure>(Session.Create(
            Guid.Empty, SessionType.Admin, null, now, now.AddMinutes(1)));
        Assert.IsType<SessionCreationResult.Failure>(Session.Create(
            Guid.NewGuid(), SessionType.User, null, now, now.AddMinutes(1)));
        Assert.IsType<SessionCreationResult.Failure>(Session.Create(
            Guid.NewGuid(), SessionType.Admin, Guid.NewGuid(), now, now.AddMinutes(1)));
        Assert.IsType<SessionCreationResult.Failure>(Session.Create(
            Guid.NewGuid(), SessionType.Admin, null, now, now));
        Assert.IsType<SessionCreationResult.Failure>(Session.Create(
            Guid.NewGuid(),
            SessionType.Admin,
            null,
            now.ToOffset(TimeSpan.FromHours(3)),
            now.AddMinutes(1)));
    }

    [Fact]
    public void IsActiveUsesInclusiveStartAndExclusiveExpiration()
    {
        DateTimeOffset now = DomainTestData.UtcNow;
        Session session = DomainTestData.CreateAdminSession(Guid.NewGuid());

        Assert.True(session.IsActive(now));
        Assert.True(session.IsActive(session.ExpiresAtUtc.AddTicks(-1)));
        Assert.False(session.IsActive(session.ExpiresAtUtc));
        Assert.False(session.IsActive(now.AddTicks(-1)));
        Assert.False(session.IsActive(now.ToOffset(TimeSpan.FromHours(3))));
    }

    [Fact]
    public void RevokeIsIdempotentAndKeepsTheFirstTimestamp()
    {
        DateTimeOffset now = DomainTestData.UtcNow;
        Session session = DomainTestData.CreateAdminSession(Guid.NewGuid());
        DateTimeOffset first = now.AddMinutes(1);

        Assert.IsType<SessionRevocationResult.Failure>(session.Revoke(now.AddTicks(-1)));
        Assert.IsType<SessionRevocationResult.Success>(session.Revoke(first));
        SessionRevocationResult.Success repeated = Assert.IsType<SessionRevocationResult.Success>(
            session.Revoke(first.AddMinutes(1)));

        Assert.Equal(first, repeated.RevokedAtUtc);
        Assert.Equal(first, session.RevokedAtUtc);
        Assert.False(session.IsActive(first));
    }
}
