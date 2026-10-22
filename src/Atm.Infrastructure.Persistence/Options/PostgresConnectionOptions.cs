using System.ComponentModel.DataAnnotations;

namespace Atm.Infrastructure.Persistence.Options;

public sealed class PostgresConnectionOptions
{
    [MinLength(1)]
    public required string Host { get; init; } = string.Empty;

    [Range(minimum: 1, maximum: 65535)]
    public required int Port { get; init; }

    public required string DataBase { get; init; } = string.Empty;

    public required string Username { get; init; } = string.Empty;

    public required string Password { get; init; } = string.Empty;

    public string ToConnectionString()
    {
        return $"Host={Host};Port={Port};Username={Username};Password={Password};Database={DataBase}";
    }
}