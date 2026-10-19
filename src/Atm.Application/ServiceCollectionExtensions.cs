using Atm.Application.Contracts.Accounts;
using Atm.Application.Contracts.Admins;
using Atm.Application.Contracts.Sessions;
using Atm.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Atm.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(
        this IServiceCollection collection)
    {
        collection.AddScoped<ISessionService, SessionService>();
        collection.AddScoped<IAdminService, AdminService>();
        collection.AddScoped<IAccountService, AccountService>();

        return collection;
    }
}