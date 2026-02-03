using Atm.Infrastructure.Repositories;
using Atm.Application.Abstractions.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Atm.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IAccountRepository, AccountRepository>();
        services.AddSingleton<ISessionRepository, SessionRepository>();
        services.AddSingleton<IOperationRepository, OperationRepository>();

        return services;
    }
}