using Atm.Domain.Accounts;
using Atm.Domain.ValueObjects;

namespace Atm.Application.Abstractions.Repositories;

public interface IAccountRepository
{
    Account? GetByNumber(AccountNumber accountNumber);

    void Add(Account account);

    void Update(Account account);
}