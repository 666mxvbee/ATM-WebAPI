using Atm.Application.Abstractions.Repositories;
using Atm.Application.Common;
using Atm.Application.Contracts.Operations.Withdraw;
using Atm.Application.Operations;
using Atm.Domain.Accounts;
using Atm.Domain.Operations;
using Atm.Domain.ValueObjects;
using NSubstitute;

namespace Atm.Application.Tests;

public sealed class WithdrawCaseTests
{
    [Fact]
    public void WithdrawWhenEnoughBalanceDecreasesBalanceAndSavesOperation()
    {
        IAccountRepository accounts = Substitute.For<IAccountRepository>();
        ISessionRepository sessions = Substitute.For<ISessionRepository>();
        IOperationRepository operations = Substitute.For<IOperationRepository>();
        var clock = new FixedClock();
        var accountNumber = new AccountNumber("aboba-2");
        var sessionId = Guid.NewGuid();
        Account account = BankingTestData.CreateAccount(accountNumber, 100m);

        sessions.GetById(sessionId).Returns(BankingTestData.CreateUserSession(sessionId, account, clock));
        accounts.GetById(account.Id).Returns(account);

        var auth = new SessionAuthorizer(sessions, clock);
        var useCase = new WithdrawUseCase(accounts, operations, auth, clock);

        WithdrawResult result = useCase.Execute(new WithdrawRequest(sessionId, 40m));

        Assert.IsType<WithdrawResult.Success>(result);
        Assert.Equal(60m, ((WithdrawResult.Success)result).Balance);
        accounts.Received(1).Update(Arg.Is<Account>(item => item.Balance.Value == 60m));
        operations.Received(1).Add(Arg.Is<OperationRecord>(operation =>
            operation.AccountNumber == accountNumber &&
            operation.Type == OperationType.Withdraw &&
            operation.Amount.Value == 40m &&
            operation.BalanceAfter.Value == 60m));
    }

    [Fact]
    public void WithdrawWhenNotEnoughBalanceReturnsFailureAndDoesNotSave()
    {
        IAccountRepository accounts = Substitute.For<IAccountRepository>();
        ISessionRepository sessions = Substitute.For<ISessionRepository>();
        IOperationRepository operations = Substitute.For<IOperationRepository>();
        var clock = new FixedClock();
        var accountNumber = new AccountNumber("aboba-666");
        var sessionId = Guid.NewGuid();
        Account account = BankingTestData.CreateAccount(accountNumber, 10m);

        sessions.GetById(sessionId).Returns(BankingTestData.CreateUserSession(sessionId, account, clock));
        accounts.GetById(account.Id).Returns(account);

        var auth = new SessionAuthorizer(sessions, clock);
        var useCase = new WithdrawUseCase(accounts, operations, auth, clock);

        WithdrawResult result = useCase.Execute(new WithdrawRequest(sessionId, 20m));

        Assert.IsType<WithdrawResult.Failure>(result);
        accounts.DidNotReceive().Update(Arg.Any<Account>());
        operations.DidNotReceive().Add(Arg.Any<OperationRecord>());
    }
}
