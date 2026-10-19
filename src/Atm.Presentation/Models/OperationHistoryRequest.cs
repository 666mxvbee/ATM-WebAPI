namespace Atm.Presentation.Models;

public sealed class OperationHistoryRequest
{
    public required Guid SessionId { get; init; }
}