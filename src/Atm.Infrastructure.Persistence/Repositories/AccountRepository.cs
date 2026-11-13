using Atm.Application.Abstractions.Repositories;
using Atm.Application.Models.Accounts;
using Atm.Application.Models.Accounts.ValueObjects;
using Npgsql;
using System.Data.Common;
using System.Runtime.CompilerServices;

namespace Atm.Infrastructure.Persistence.Repositories;

internal class AccountRepository : IAccountRepository
{
    private readonly NpgsqlDataSource _dataSource;

    public AccountRepository(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public async IAsyncEnumerable<Account> AddAccountAsync(
        IReadOnlyCollection<Account> account,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        if (account.Count == 0)
        {
            yield break;
        }

        await using NpgsqlConnection connection = await _dataSource
            .OpenConnectionAsync(cancellationToken);

        const string sql = """
                           INSERT INTO accounts (account_number, pincode, balance)
                           SELECT input.account_number, input.pincode, input.balance
                           FROM UNNEST(:account_numbers, :pincodes, :balances)
                           AS input(account_number, pincode, balance)
                           RETURNING account_id, account_number, pincode, balance;
                           """;

        Account[] accounts = [.. account];

        string[] numbers = [.. accounts.Select(x => x.Number.Value)];
        string[] pins = [.. accounts.Select(x => x.Pincode.Value)];
        decimal[] balances = [.. accounts.Select(x => x.Balance.Value)];

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.Add(new NpgsqlParameter("account_numbers", numbers));
        command.Parameters.Add(new NpgsqlParameter("pincodes", pins));
        command.Parameters.Add(new NpgsqlParameter("balances", balances));

        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            yield return MapReaderToAccount(reader);
        }
    }

    public async Task<Account?> FindAccountByIdAsync(
        AccountId accountId,
        CancellationToken cancellationToken)
    {
        await using NpgsqlConnection connection = await _dataSource
            .OpenConnectionAsync(cancellationToken);

        const string sql = """
                           SELECT account_id, account_number, pincode, balance
                           FROM accounts
                           WHERE account_id = :account_id;
                           """;

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.Add(new NpgsqlParameter("account_id", accountId.Value));

        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        if (await reader.ReadAsync(cancellationToken) is false)
        {
            return null;
        }

        return MapReaderToAccount(reader);
    }

    public async Task<Account?> FindAccountByNumberAsync(
        AccountNumber accountNumber,
        CancellationToken cancellationToken)
    {
        await using NpgsqlConnection connection = await _dataSource
            .OpenConnectionAsync(cancellationToken);

        const string sql = """
                           SELECT account_id, account_number, pincode, balance
                           FROM accounts
                           WHERE account_number = :account_number;
                           """;

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.Add(new NpgsqlParameter("account_number", accountNumber.Value));

        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        if (await reader.ReadAsync(cancellationToken) is false)
        {
            return null;
        }

        return MapReaderToAccount(reader);
    }

    public async IAsyncEnumerable<Account> UpdateAccountAsync(
        IReadOnlyCollection<Account> account,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        if (account.Count == 0)
        {
            yield break;
        }

        await using NpgsqlConnection connection = await _dataSource
            .OpenConnectionAsync(cancellationToken);

        const string sql = """
                           UPDATE accounts AS target
                           SET balance = input.balance
                           FROM UNNEST(:account_ids, :balances)
                           AS input(account_id, balance)
                           WHERE target.account_id = input.account_id
                           RETURNING target.account_id, target.account_number, target.pincode, target.balance;
                           """;

        Account[] accounts = [.. account];
        long[] accountIds = [.. accounts.Select(x => x.Id.Value)];
        decimal[] balances = [.. accounts.Select(x => x.Balance.Value)];

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.Add(new NpgsqlParameter("account_ids", accountIds));
        command.Parameters.Add(new NpgsqlParameter("balances", balances));

        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            yield return MapReaderToAccount(reader);
        }
    }

    private static Account MapReaderToAccount(DbDataReader reader)
    {
        return Account.CreateFromDb(
            AccountId.Create(reader.GetInt64(0)),
            AccountNumber.Create(reader.GetString(1)),
            Pincode.Create(reader.GetString(2)),
            Money.Create(reader.GetDecimal(3)));
    }
}