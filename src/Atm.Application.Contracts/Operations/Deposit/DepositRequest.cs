namespace Atm.Application.Contracts.Operations.Deposit;

public sealed record DepositRequest(Guid UserSessionId, decimal Amount);