namespace Atm.Application.Contracts.Sessions.CreateAdminSession;

public sealed record CreateAdminSessionRequest(string SystemPassword, string AccountNumber);