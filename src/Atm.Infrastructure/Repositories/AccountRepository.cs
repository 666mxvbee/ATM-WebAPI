using Atm.Application.Abstractions.Repositories;
using Atm.Domain.Accounts;
using Atm.Domain.ValueObjects;

namespace Atm.Infrastructure.Repositories;

public sealed class AccountRepository : IAccountRepository
{
    private readonly Dictionary<string, Account> _accounts = new();

    public Account? GetByNumber(AccountNumber accountNumber)
    {
        return _accounts.GetValueOrDefault(accountNumber.Value);
    }

    public void Add(Account account)
    {
        _accounts[account.Number.Value] = account;
    }

    public void Update(Account account)
    {
        _accounts[account.Number.Value] = account;
    }
}