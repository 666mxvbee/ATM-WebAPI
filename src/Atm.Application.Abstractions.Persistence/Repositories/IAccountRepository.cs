using Atm.Application.Models.Accounts;
using Atm.Application.Models.Accounts.ValueObjects;

namespace Atm.Application.Abstractions.Persistence.Repositories;

public interface IAccountRepository
{
    Task<Account> AddAccounts(
        IReadOnlyCollection<Account> accounts,
        CancellationToken cancellationToken);

    Task<Account> GetAccountById(
        AccountId accountId,
        CancellationToken cancellationToken);

    Task<Account> GetAccountByNumber(
        AccountNumber accountNumber,
        CancellationToken cancellationToken);

    Task UpdateAccount(
        Account account,
        CancellationToken cancellationToken);
}