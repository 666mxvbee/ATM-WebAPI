using Atm.Application.Models.Accounts;
using Atm.Application.Models.Accounts.ValueObjects;

namespace Atm.Application.Abstractions.Repositories;

public interface IAccountRepository
{
    IAsyncEnumerable<Account> AddAccountAsync(
        IReadOnlyCollection<Account> account,
        CancellationToken cancellationToken);

    Task<Account?> FindAccountByIdAsync(
        AccountId accountId,
        CancellationToken cancellationToken);

    Task<Account?> FindAccountByNumberAsync(
        AccountNumber accountNumber,
        CancellationToken cancellationToken);

    IAsyncEnumerable<Account> UpdateAccountAsync(
        IReadOnlyCollection<Account> account,
        CancellationToken cancellationToken);
}