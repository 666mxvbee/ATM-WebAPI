using Atm.Application.Abstractions.Repositories;
using Atm.Application.Accounts;
using Atm.Application.Common;
using Atm.Application.Contracts.Accounts.CreateAccount;
using Atm.Application.Contracts.Sessions.CreateUserSession;
using Atm.Application.Sessions;
using Atm.Domain.Accounts;
using Atm.Domain.Sessions;
using Atm.Domain.ValueObjects;
using Atm.Infrastructure.Security;
using Microsoft.Extensions.Options;

namespace Atm.Application.Tests;

public sealed class AccountAuthenticationFlowTests
{
    [Fact]
    public void CreatedAccountStoresOnlyHashAndAcceptsOnlyTheCorrectPin()
    {
        var accounts = new TestAccountRepository();
        var sessions = new TestSessionRepository();
        var clock = new FixedClock();
        var hasher = new PinHasher();
        var adminSessionId = Guid.NewGuid();
        var ownerUserId = Guid.NewGuid();
        sessions.Add(BankingTestData.CreateAdminSession(adminSessionId, clock));

        var authorizer = new SessionAuthorizer(sessions, clock);
        var createAccount = new CreateAccountUseCase(accounts, authorizer, hasher, clock);
        CreateAccountResult created = createAccount.Execute(new CreateAccountRequest(
            adminSessionId, " account-01 ", "1234", 1_000_000_000_000_000m, ownerUserId));

        Assert.IsType<CreateAccountResult.Success>(created);
        Account storedAccount = Assert.IsType<Account>(accounts.Account);
        Assert.Equal("ACCOUNT-01", storedAccount.Number.NormalizedValue);
        Assert.Equal(ownerUserId, storedAccount.OwnerUserId);
        Assert.NotEqual("1234", storedAccount.PinHash.Value);
        Assert.True(hasher.Verify(storedAccount.PinHash, new PinCode("1234")));

        IOptions<SessionOptions> options = Options.Create(new SessionOptions
        {
            UserLifetimeMinutes = 30,
            AdminLifetimeMinutes = 15,
        });
        var createSession = new CreateUserSessionUseCase(sessions, accounts, hasher, clock, options);

        Assert.IsType<CreateUserSessionResult.Unauthorized>(createSession.Execute(
            new CreateUserSessionRequest("account-01", "4321")));
        CreateUserSessionResult.Success signedIn = Assert.IsType<CreateUserSessionResult.Success>(
            createSession.Execute(new CreateUserSessionRequest("account-01", "1234")));

        Session userSession = Assert.IsType<Session>(sessions.GetById(signedIn.SessionId));
        Assert.Equal(storedAccount.Id, userSession.AccountId);
        Assert.True(userSession.IsActive(clock.UtcNow));
        Assert.Equal(clock.UtcNow.AddMinutes(30), userSession.ExpiresAtUtc);
    }

    private sealed class TestAccountRepository : IAccountRepository
    {
        public Account? Account { get; private set; }

        public Account? GetById(Guid id)
        {
            return Account?.Id == id ? Account : null;
        }

        public Account? GetByNumber(AccountNumber accountNumber)
        {
            return Account?.Number.NormalizedValue == accountNumber.NormalizedValue ? Account : null;
        }

        public void Add(Account account)
        {
            Account = account;
        }

        public void Update(Account account)
        {
            Account = account;
        }
    }

    private sealed class TestSessionRepository : ISessionRepository
    {
        private readonly Dictionary<Guid, Session> _sessions = new();

        public Session? GetById(Guid id)
        {
            return _sessions.GetValueOrDefault(id);
        }

        public void Add(Session session)
        {
            _sessions.Add(session.Id, session);
        }
    }
}
