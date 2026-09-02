using Atm.Application.Abstractions.Time;

namespace Atm.Application.Tests;

internal sealed class FixedClock : IClock
{
    public DateTimeOffset UtcNow { get; set; } = new(2026, 9, 2, 12, 0, 0, TimeSpan.Zero);
}
