using Atm.Application.Contracts.Admins.Operations;
using Atm.Application.Models.Accounts;

namespace Atm.HighLoad.Application.Mapping;

public static class AccountMappingExtensions
{
    public static AccountDto AccountMapToDto(
        this Account account)
    {
        return new AccountDto(account.Number.Value);
    }
}