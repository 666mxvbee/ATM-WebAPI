namespace Atm.Presentation.Models;

public sealed class WithdrawRequest
{
    public required Guid SessionId { get; init; }

    public required decimal Money { get; init; }
}