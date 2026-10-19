namespace Atm.Presentation.Models;

public sealed class BalanceRequest
{
    public required Guid SessionId { get; init; }
}