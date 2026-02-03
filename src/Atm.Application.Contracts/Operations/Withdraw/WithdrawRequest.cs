namespace Atm.Application.Contracts.Operations.Withdraw;

public sealed record WithdrawRequest(Guid UserSessionId, decimal Amount);