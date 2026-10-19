using Atm.Application.Contracts.Accounts.Models;
using Atm.Application.Models.Operations;

namespace Atm.Application.Mapping;

public static class OperationsMappingExtensions
{
    public static IReadOnlyCollection<OperationHistoryDto> HistoryToDto(
        this IEnumerable<Operation> operations)
    {
        return
        [
            .. operations
                .Select(operation => new OperationHistoryDto(
                    operation.OperationId.Value,
                    operation.AccountId.Value,
                    operation.Time,
                    operation.OperationType.ToString(),
                    operation.Amount.Value,
                    operation.BalanceAfter.Value))
        ];
    }
}