using Atm.Application.Abstractions.Repositories;
using Atm.Application.Common;
using Atm.Application.Contracts.Operations.Withdraw;
using Atm.Application.Operations;
using Atm.Domain.Accounts;
using Atm.Domain.Operations;
using Atm.Domain.Sessions;
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

        var accountNumber = new AccountNumber("aboba-2");
        var sessionId = Guid.NewGuid();

        sessions.GetById(sessionId).Returns(new Session(sessionId, SessionType.User, accountNumber));

        var account = new Account(accountNumber, new PinCode("6666"), new Money(100m));
        accounts.GetByNumber(accountNumber).Returns(account);

        var auth = new SessionAuthorizer(sessions);
        var useCase = new WithdrawUseCase(accounts, operations, auth);

        WithdrawResult result = useCase.Execute(new WithdrawRequest(sessionId, 40m));

        Assert.IsType<WithdrawResult.Success>(result);
        Assert.Equal(60m, ((WithdrawResult.Success)result).Balance);

        accounts.Received(1).Update(Arg.Is<Account>(a => a.Balance.Value == 60m));

        operations.Received(1).Add(Arg.Is<OperationRecord>(op =>
            op.AccountNumber == accountNumber &&
            op.Type == OperationType.Withdraw &&
            op.Amount.Value == 40m &&
            op.BalanceAfter.Value == 60m));
    }

    [Fact]
    public void WithdrawWhenNotEnoughBalanceReturnsFailureAndDoesNotSave()
    {
        IAccountRepository accounts = Substitute.For<IAccountRepository>();
        ISessionRepository sessions = Substitute.For<ISessionRepository>();
        IOperationRepository operations = Substitute.For<IOperationRepository>();

        var accountNumber = new AccountNumber("aboba-666");
        var sessionId = Guid.NewGuid();

        sessions.GetById(sessionId).Returns(new Session(sessionId, SessionType.User, accountNumber));

        var account = new Account(accountNumber, new PinCode("9999"), new Money(10m));
        accounts.GetByNumber(accountNumber).Returns(account);

        var auth = new SessionAuthorizer(sessions);
        var useCase = new WithdrawUseCase(accounts, operations, auth);

        WithdrawResult result = useCase.Execute(new WithdrawRequest(sessionId, 20m));

        Assert.IsType<WithdrawResult.Failure>(result);
        accounts.DidNotReceive().Update(Arg.Any<Account>());
        operations.DidNotReceive().Add(Arg.Any<OperationRecord>());
    }
}