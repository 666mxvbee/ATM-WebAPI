using Atm.Application.Abstractions.Repositories;
using Atm.Infrastructure.Persistence.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Atm.Infrastructure.Persistence;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection collection)
    {
        collection.AddScoped<IAccountRepository, AccountRepository>();
        collection.AddScoped<IAdminSessionRepository, AdminSessionRepository>();
        collection.AddScoped<IUserSessionRepository, UserSessionRepository>();
        collection.AddScoped<IOperationHistoryRepository, OperationHistoryRepository>();

        return collection;
    }
}