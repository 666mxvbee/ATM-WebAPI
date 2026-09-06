using Atm.Infrastructure.Persistence.DesignTime;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Atm.Infrastructure.Tests.Persistence;

[Collection("Environment configuration")]
public sealed class DesignTimeFactoryTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("UnknownOption=secret-value")]
    public void InvalidConfigurationFailsWithoutExposingValues(string? value)
    {
        string? previous = Environment.GetEnvironmentVariable(AtmDbContextFactory.ConnectionStringVariable);
        try
        {
            Environment.SetEnvironmentVariable(AtmDbContextFactory.ConnectionStringVariable, value);
            var factory = new AtmDbContextFactory();
            InvalidOperationException error = Assert.Throws<InvalidOperationException>(() => factory.CreateDbContext([]));
            Assert.DoesNotContain("secret-value", error.ToString(), StringComparison.Ordinal);
            Assert.Null(error.InnerException);
        }
        finally
        {
            Environment.SetEnvironmentVariable(AtmDbContextFactory.ConnectionStringVariable, previous);
        }
    }

    [Fact]
    public void FactoryCreatesNpgsqlContextWithoutOpeningAConnection()
    {
        string? previous = Environment.GetEnvironmentVariable(AtmDbContextFactory.ConnectionStringVariable);
        try
        {
            var settings = new NpgsqlConnectionStringBuilder { Host = "localhost", Database = "model_tests" };
            Environment.SetEnvironmentVariable(AtmDbContextFactory.ConnectionStringVariable, settings.ConnectionString);
            using Infrastructure.Persistence.AtmDbContext context = new AtmDbContextFactory().CreateDbContext([]);
            Assert.Equal("Npgsql.EntityFrameworkCore.PostgreSQL", context.Database.ProviderName);
            Assert.Equal(System.Data.ConnectionState.Closed, context.Database.GetDbConnection().State);
            Assert.NotEmpty(context.Model.GetEntityTypes());
        }
        finally
        {
            Environment.SetEnvironmentVariable(AtmDbContextFactory.ConnectionStringVariable, previous);
        }
    }
}

[CollectionDefinition("Environment configuration", DisableParallelization = true)]
public sealed class EnvironmentConfigurationCollection;
