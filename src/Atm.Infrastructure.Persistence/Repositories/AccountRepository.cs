using Atm.Application.Abstractions.Repositories;
using Atm.Application.Models.Accounts;
using Atm.Application.Models.Accounts.ValueObjects;

namespace Atm.Infrastructure.Persistence.Repositories;

public sealed class AccountRepository : IAccountRepository
{
    private readonly Dictionary<AccountId, Account> _values = [];
    private readonly Lock _sync = new();
    private long _lastId;


    public Task<Account> AddAccountAsync(
        Account account,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_sync)
        {
            if (_values.Values.Any(x => x.Number == account.Number))
            {
                throw new InvalidOperationException(
                    "Account number already exists");
            }

            var savedAccount = Account.CreateFromDb(
                AccountId.Create(++_lastId),
                account.Number,
                account.Pincode,
                account.Balance);

            _values.Add(savedAccount.Id, Copy(account));

            return Task.FromResult(savedAccount);
        }
    }

    public Task<Account?> FindAccountByIdAsync(
        AccountId accountId,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_sync)
        {
            Account? result = _values.TryGetValue(accountId, out Account? account)
                ? Copy(account)
                : null;

            return Task.FromResult(result);
        }
    }

    public Task<Account?> FindAccountByNumberAsync(
        AccountNumber accountNumber,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_sync)
        {
            Account? account = _values.Values
                .FirstOrDefault(x => x.Number == accountNumber);

            return Task.FromResult(
                account is null ? null : Copy(account));
        }
    }

    public Task UpdateAccountAsync(
        Account account,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_sync)
        {
            if (!_values.ContainsKey(account.Id))
            {
                throw new InvalidOperationException("Account not found");
            }

            _values[account.Id] = Copy(account);
        }
        
        return Task.CompletedTask;
    }

    private static Account Copy(Account account)
    {
        return Account.CreateFromDb(
            account.Id,
            account.Number,
            account.Pincode,
            account.Balance);
    }
}