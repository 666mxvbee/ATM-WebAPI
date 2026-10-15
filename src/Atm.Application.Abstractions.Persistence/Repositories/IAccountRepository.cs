using Atm.Application.Models.Accounts;
using Atm.Application.Models.Accounts.ValueObjects;

namespace Atm.Application.Abstractions.Persistence.Repositories;

public interface IAccountRepository
{
    Task<Account> AddAccountsAsync(
        IReadOnlyCollection<Account> accounts,
        CancellationToken cancellationToken);

    Task<Account?> FindAccountByIdAsync(
        AccountId accountId,
        CancellationToken cancellationToken);

    Task<Account?> FindAccountByNumberAsync(
        AccountNumber accountNumber,
        CancellationToken cancellationToken);

    Task UpdateAccountAsync(
        Account account,
        CancellationToken cancellationToken);
}