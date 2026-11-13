using Atm.Application.Abstractions.Repositories;
using Atm.Application.Models.Accounts.ValueObjects;
using Atm.Application.Models.Operations;
using Atm.Application.Models.Operations.ValueObjects;
using Npgsql;
using System.Runtime.CompilerServices;

namespace Atm.Infrastructure.Persistence.Repositories;

internal class OperationHistoryRepository : IOperationHistoryRepository
{
    private readonly NpgsqlDataSource _dataSource;

    public OperationHistoryRepository(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public async IAsyncEnumerable<Operation> AddOperationAsync(
        IReadOnlyCollection<Operation> operation,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        if (operation.Count == 0)
        {
            yield break;
        }

        await using NpgsqlConnection connection = await _dataSource
            .OpenConnectionAsync(cancellationToken);

        const string sql = """
                           INSERT INTO operation_history (account_id, time, operation_type, amount, balance_after)
                           SELECT input.account_id, input.time, input.operation_type, input.amount, input.balance_after
                           FROM UNNEST(:account_ids, :times, :operation_types, :amounts, :balances_after)
                           AS input(account_id, time, operation_type, amount, balance_after)
                           RETURNING operation_id, account_id, time, operation_type, amount, balance_after;
                           """;

        Operation[] operations = [.. operation];
        long[] accountIds = [.. operations.Select(x => x.AccountId.Value)];
        DateTimeOffset[] times = [.. operation.Select(x => x.Time)];
        int[] operationTypes = [.. operation.Select(x => (int)x.OperationType)];
        decimal[] amount = [.. operation.Select(x => x.Amount.Value)];
        decimal[] balanceAfter = [.. operation.Select(x => x.BalanceAfter.Value)];

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.Add(new NpgsqlParameter("account_ids", accountIds));
        command.Parameters.Add(new NpgsqlParameter("times", times));
        command.Parameters.Add(new NpgsqlParameter("operation_types", operationTypes));
        command.Parameters.Add(new NpgsqlParameter("amounts", amount));
        command.Parameters.Add(new NpgsqlParameter("balances_after", balanceAfter));

        await using NpgsqlDataReader reader = await command
            .ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            yield return MapReaderToOperation(reader);
        }
    }

    public async IAsyncEnumerable<Operation> FindByAccountIdAsync(
        AccountId accountId,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await using NpgsqlConnection connection = await _dataSource.OpenConnectionAsync(cancellationToken);

        const string sql = """
                           SELECT operation_id, account_id, time, operation_type, amount, balance_after
                           FROM operation_history
                           WHERE account_id = :account_id
                           ORDER BY time DESC, operation_id DESC;
                           """;

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.Add(new NpgsqlParameter("account_id", accountId.Value));

        await using NpgsqlDataReader reader = await command
            .ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            yield return MapReaderToOperation(reader);
        }
    }

    private static Operation MapReaderToOperation(NpgsqlDataReader reader)
    {
        return Operation.CreateFromDb(
            OperationId.Create(reader.GetInt64(0)),
            AccountId.Create(reader.GetInt64(1)),
            reader.GetFieldValue<DateTimeOffset>(2),
            (OperationType)reader.GetInt32(3),
            Money.Create(reader.GetDecimal(4)),
            Money.Create(reader.GetDecimal(5)));
    }
}