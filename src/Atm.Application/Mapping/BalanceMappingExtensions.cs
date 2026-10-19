using Atm.Application.Contracts.Accounts.Models;
using Atm.Application.Models.Accounts.ValueObjects;

namespace Atm.Application.Mapping;

public static class BalanceMappingExtensions
{
    public static BalanceDto BalanceToDto(
        this Money balance)
    {
        return new BalanceDto(balance.Value);
    }
}