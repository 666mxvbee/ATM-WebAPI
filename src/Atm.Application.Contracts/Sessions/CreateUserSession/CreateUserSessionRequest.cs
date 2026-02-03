namespace Atm.Application.Contracts.Sessions.CreateUserSession;

public sealed record CreateUserSessionRequest(string AccountNumber, string Pin);