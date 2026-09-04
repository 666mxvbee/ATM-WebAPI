namespace Atm.Application.Sessions;

public sealed class SessionOptions
{
    public const string SectionName = "Sessions";

    public int UserLifetimeMinutes { get; set; }

    public int AdminLifetimeMinutes { get; set; }
}
