using Atm.Application.Accounts;
using Atm.Application.Common;
using Atm.Application.Contracts.Accounts.CreateAccount;
using Atm.Application.Contracts.Accounts.GetBalance;
using Atm.Application.Contracts.Operations.Deposit;
using Atm.Application.Contracts.Operations.GetTransactionHistory;
using Atm.Application.Contracts.Operations.Withdraw;
using Atm.Application.Contracts.Sessions.CreateAdminSession;
using Atm.Application.Contracts.Sessions.CreateUserSession;
using Atm.Application.Operations;
using Atm.Application.Sessions;
using Microsoft.Extensions.DependencyInjection;

namespace Atm.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSingleton<SessionAuthorizer>();

        services.AddTransient<ICreateAdminSessionPort, CreateAdminSessionUseCase>();
        services.AddTransient<ICreateUserSessionPort, CreateUserSessionUseCase>();

        services.AddTransient<ICreateAccountPort, CreateAccountUseCase>();
        services.AddTransient<IGetBalancePort, GetBalanceUseCase>();

        services.AddTransient<IDepositPort, DepositUseCase>();
        services.AddTransient<IWithdrawPort, WithdrawUseCase>();
        services.AddTransient<IGetTransactionHistoryPort, GetTransactionHistoryUseCase>();

        return services;
    }
}