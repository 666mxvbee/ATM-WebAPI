using Atm.Application.Abstractions.Repositories;
using Atm.Domain.Operations;
using Atm.Domain.ValueObjects;

namespace Atm.Infrastructure.Repositories;

public sealed class OperationRepository : IOperationRepository
{
    private readonly List<OperationRecord> _operations = new();

    public void Add(OperationRecord operation)
    {
        _operations.Add(operation);
    }

    public IReadOnlyList<OperationRecord> GetByAccountNumber(AccountNumber accountNumber)
    {
        var result = new List<OperationRecord>();

        foreach (OperationRecord operation in _operations)
        {
            if (operation.AccountNumber.Value == accountNumber.Value)
            {
                result.Add(operation);
            }
        }

        return result;
    }
}