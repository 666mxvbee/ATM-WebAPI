using Atm.Application.Abstractions.Repositories;
using Atm.Domain.Accounts;
using Atm.Domain.ValueObjects;

namespace Atm.Infrastructure.Repositories;

public sealed class AccountRepository : IAccountRepository
{
    private readonly Dictionary<Guid, Account> _accounts = new();

    public Account? GetById(Guid id)
    {
        return _accounts.GetValueOrDefault(id);
    }

    public Account? GetByNumber(AccountNumber accountNumber)
    {
        return _accounts.Values.FirstOrDefault(
            account => account.Number.NormalizedValue == accountNumber.NormalizedValue);
    }

    public void Add(Account account)
    {
        _accounts.Add(account.Id, account);
    }

    public void Update(Account account)
    {
        _accounts[account.Id] = account;
    }
}
