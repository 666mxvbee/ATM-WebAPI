using Atm.Domain.Operations;
using Atm.Domain.ValueObjects;

namespace Atm.Application.Abstractions.Repositories;

public interface IOperationRepository
{
    void Add(OperationRecord operation);

    IReadOnlyList<OperationRecord> GetByAccountNumber(AccountNumber accountNumber);
}