namespace Atm.Application.Contracts.Accounts.CreateAccount;

public sealed record CreateAccountRequest(
    Guid AdminSessionId,
    string AccountNumber,
    string Pin,
    decimal InitialBalance);