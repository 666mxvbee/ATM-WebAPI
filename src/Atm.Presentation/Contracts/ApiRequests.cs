using System.Text.Json.Serialization;

namespace Atm.Presentation.Contracts;

public sealed record CreateAccountApiRequest(
    [property: JsonPropertyName("adminSessionId")] Guid AdminSessionId,
    [property: JsonPropertyName("accountNumber")] string AccountNumber,
    [property: JsonPropertyName("pin")] string Pin,
    [property: JsonPropertyName("initialBalance")] decimal InitialBalance);

public sealed record AmountApiRequest(
    [property: JsonPropertyName("userSessionId")] Guid UserSessionId,
    [property: JsonPropertyName("amount")] decimal Amount);

public sealed record CreateUserSessionApiRequest(
    [property: JsonPropertyName("accountNumber")] string AccountNumber,
    [property: JsonPropertyName("pin")] string Pin);

public sealed record CreateAdminSessionApiRequest(
    [property: JsonPropertyName("systemPassword")] string SystemPassword,
    [property: JsonPropertyName("accountNumber")] string AccountNumber);
