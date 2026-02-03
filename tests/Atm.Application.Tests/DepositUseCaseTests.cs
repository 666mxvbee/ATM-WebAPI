using Atm.Application.Abstractions.Repositories;
using Atm.Application.Common;
using Atm.Application.Contracts.Operations.Deposit;
using Atm.Application.Operations;
using Atm.Domain.Accounts;
using Atm.Domain.Operations;
using Atm.Domain.Sessions;
using Atm.Domain.ValueObjects;
using NSubstitute;

namespace Atm.Application.Tests;

public sealed class DepositUseCaseTests
{
    [Fact]
    public void DepositWhenValidIncreaseBalanceAndSavesOperation()
    {
        IAccountRepository accounts = Substitute.For<IAccountRepository>();
        ISessionRepository sessions = Substitute.For<ISessionRepository>();
        IOperationRepository operations = Substitute.For<IOperationRepository>();

        var accountNumber = new AccountNumber("aboba");
        var sessionId = Guid.NewGuid();

        sessions.GetById(sessionId).Returns(new Session(sessionId, SessionType.User, accountNumber));

        var account = new Account(accountNumber, new PinCode("1234"), new Money(100m));
        accounts.GetByNumber(accountNumber).Returns(account);

        var auth = new SessionAuthorizer(sessions);
        var useCase = new DepositUseCase(accounts, operations, auth);

        DepositResult result = useCase.Execute(new DepositRequest(sessionId, 50m));

        Assert.IsType<DepositResult.Success>(result);
        Assert.Equal(150m, ((DepositResult.Success)result).Balance);

        accounts.Received(1).Update(Arg.Is<Account>(a => a.Balance.Value == 150m));

        operations.Received(1).Add(Arg.Is<OperationRecord>(op =>
            op.AccountNumber == accountNumber &&
            op.Type == OperationType.Deposit &&
            op.Amount.Value == 50m &&
            op.BalanceAfter.Value == 150m));
    }
}