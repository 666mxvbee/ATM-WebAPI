using Atm.Domain.ValueObjects;

namespace Atm.Application.Contracts.Sessions.CreateAdminSession;

public sealed record CreateAdminSessionRequest(string SystemPassword, AccountNumber AccountNumber);