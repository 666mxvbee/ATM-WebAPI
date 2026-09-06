using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Npgsql;

namespace Atm.Infrastructure.Persistence.DesignTime;

public sealed class AtmDbContextFactory : IDesignTimeDbContextFactory<AtmDbContext>
{
    public const string ConnectionStringVariable = "ConnectionStrings__AtmDatabase";

    public AtmDbContext CreateDbContext(string[] args)
    {
        string? connectionString = Environment.GetEnvironmentVariable(ConnectionStringVariable);
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException($"Set {ConnectionStringVariable} before running EF tools.");

        NpgsqlConnectionStringBuilder settings;
        try
        {
            settings = new NpgsqlConnectionStringBuilder(connectionString);
        }
        catch (ArgumentException)
        {
            throw new InvalidOperationException("The database connection configuration is invalid.");
        }

        if (string.IsNullOrWhiteSpace(settings.Host) || string.IsNullOrWhiteSpace(settings.Database))
            throw new InvalidOperationException("The database connection must specify Host and Database.");

        DbContextOptions<AtmDbContext> options = new DbContextOptionsBuilder<AtmDbContext>()
            .UseNpgsql(settings.ConnectionString)
            .Options;
        return new AtmDbContext(options);
    }
}
