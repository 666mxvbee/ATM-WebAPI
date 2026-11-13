using Atm.Application.Abstractions;
using Atm.Application.Abstractions.Repositories;
using Atm.Application.Abstractions.Transactions;
using Atm.Infrastructure.Persistence.Options;
using Atm.Infrastructure.Persistence.Repositories;
using Atm.Infrastructure.Persistence.Transactions;
using FluentMigrator.Runner;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Atm.Infrastructure.Persistence;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection collection,
        IConfiguration configuration)
    {
        IConfigurationSection postgresSection = configuration.GetSection("Infrastructure:Persistence");

        collection.AddOptions<PostgresConnectionOptions>()
            .Bind(postgresSection)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        PostgresConnectionOptions? postgresOptions = postgresSection.Get<PostgresConnectionOptions>()
            ?? throw new InvalidOperationException("PostgresConnectionOptions not found");

        string connectionString = postgresOptions.ToConnectionString();

        collection.AddNpgsqlDataSource(connectionString);

        collection
            .AddFluentMigratorCore()
            .ConfigureRunner(runner => runner
                .AddPostgres()
                .WithGlobalConnectionString(connectionString)
                .WithMigrationsIn(typeof(IAssemblyMarker).Assembly))
            .AddLogging(logging => logging.AddFluentMigratorConsole());

        collection.AddSingleton<IAccountRepository, AccountRepository>();
        collection.AddSingleton<IAdminSessionRepository, AdminSessionRepository>();
        collection.AddSingleton<IUserSessionRepository, UserSessionRepository>();
        collection.AddSingleton<IOperationHistoryRepository, OperationHistoryRepository>();

        collection.AddScoped<IPersistenceContext, PersistenceContext>();
        collection.AddScoped<IPersistenceTransactionProvider, PostgresPersistenceTransactionProvider>();
        return collection;
    }
}