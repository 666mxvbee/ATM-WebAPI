using Atm.Application.Abstractions;
using Atm.Application.Abstractions.Repositories;
using Atm.Infrastructure.Persistence.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Atm.Infrastructure.Persistence;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection collection)
    {
        collection.AddSingleton<IAccountRepository, AccountRepository>();
        collection.AddSingleton<IAdminSessionRepository, AdminSessionRepository>();
        collection.AddSingleton<IUserSessionRepository, UserSessionRepository>();
        collection.AddSingleton<IOperationHistoryRepository, OperationHistoryRepository>();

        collection.AddScoped<IPersistenceContext, PersistenceContext>();

        return collection;
    }
}