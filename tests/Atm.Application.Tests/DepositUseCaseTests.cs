using Atm.Application.Abstractions.Repositories;
using Atm.Application.Common;
using Atm.Application.Contracts.Operations.Deposit;
using Atm.Application.Operations;
using Atm.Domain.Accounts;
using Atm.Domain.Operations;
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
        var clock = new FixedClock();
        var accountNumber = new AccountNumber("aboba");
        var sessionId = Guid.NewGuid();
        Account account = BankingTestData.CreateAccount(accountNumber, 100m);

        sessions.GetById(sessionId).Returns(BankingTestData.CreateUserSession(sessionId, account, clock));
        accounts.GetById(account.Id).Returns(account);

        var auth = new SessionAuthorizer(sessions, clock);
        var useCase = new DepositUseCase(accounts, operations, auth, clock);

        DepositResult result = useCase.Execute(new DepositRequest(sessionId, 50m));

        Assert.IsType<DepositResult.Success>(result);
        Assert.Equal(150m, ((DepositResult.Success)result).Balance);
        accounts.Received(1).Update(Arg.Is<Account>(item => item.Balance.Value == 150m));
        operations.Received(1).Add(Arg.Is<OperationRecord>(operation =>
            operation.AccountNumber == accountNumber &&
            operation.Type == OperationType.Deposit &&
            operation.Amount.Value == 50m &&
            operation.BalanceAfter.Value == 150m));
    }
}
