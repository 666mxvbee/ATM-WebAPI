using Atm.Application.Abstractions.Repositories;
using Atm.Application.Abstractions.Security;
using Atm.Application.Abstractions.Time;
using Atm.Infrastructure.Repositories;
using Atm.Infrastructure.Security;
using Atm.Infrastructure.Time;
using Microsoft.Extensions.DependencyInjection;

namespace Atm.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IAccountRepository, AccountRepository>();
        services.AddSingleton<ISessionRepository, SessionRepository>();
        services.AddSingleton<IOperationRepository, OperationRepository>();
        services.AddSingleton<IClock, SystemClock>();
        services.AddSingleton<IPinHasher, PinHasher>();

        return services;
    }
}
