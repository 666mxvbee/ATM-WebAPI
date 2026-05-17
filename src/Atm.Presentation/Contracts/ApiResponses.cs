using Atm.Application.Contracts.Operations.GetTransactionHistory;
using System.Text.Json.Serialization;

namespace Atm.Presentation.Contracts;

public sealed record ErrorResponse(
    [property: JsonPropertyName("error")] string Error);
    
public sealed record SessionResponse(
    [property: JsonPropertyName("sessionId")] Guid SessionId);
    
public sealed record BalanceResponse(
    [property: JsonPropertyName("balance")] decimal Balance);

public sealed record MessageResponse(
    [property: JsonPropertyName("message")] string Message);

public sealed record TransactionsResponse(
    [property: JsonPropertyName("transactions")] IReadOnlyList<TransactionDto> Transactions);
    