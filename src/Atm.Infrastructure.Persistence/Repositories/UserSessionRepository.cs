using Atm.Application.Abstractions.Repositories;
using Atm.Application.Models.Accounts.ValueObjects;
using Atm.Application.Models.Sessions;
using Npgsql;
using System.Data.Common;
using System.Runtime.CompilerServices;

namespace Atm.Infrastructure.Persistence.Repositories;

internal class UserSessionRepository : IUserSessionRepository
{
    private readonly NpgsqlDataSource _dataSource;

    public UserSessionRepository(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public async IAsyncEnumerable<UserSession> AddSessionAsync(
        IReadOnlyCollection<UserSession> session,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        if (session.Count == 0)
        {
            yield break;
        }

        await using NpgsqlConnection connection = await _dataSource
            .OpenConnectionAsync(cancellationToken);

        const string sql = """
                           INSERT INTO user_sessions (user_session_id, account_id)
                           SELECT *
                           FROM UNNEST(:user_session_ids, :account_ids)
                           RETURNING user_session_id, account_id;
                           """;

        Guid[] userSessionsIds = [.. session.Select(x => x.SessionId)];
        long[] accountIds = [.. session.Select(x => x.AccountId.Value)];

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.Add(new NpgsqlParameter("user_session_ids", userSessionsIds));
        command.Parameters.Add(new NpgsqlParameter("account_ids", accountIds));

        await using DbDataReader reader = await command
            .ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            yield return new UserSession(
                reader.GetGuid(0),
                AccountId.Create(reader.GetInt64(1)));
        }
    }

    public async Task<UserSession?> FindUserSessionByIdAsync(
        Guid sessionId,
        CancellationToken cancellationToken)
    {
        await using NpgsqlConnection connection = await _dataSource
            .OpenConnectionAsync(cancellationToken);

        const string sql = """
                           SELECT *
                           FROM user_sessions
                           WHERE user_session_id = :session_id;
                           """;

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.Add(new NpgsqlParameter("session_id", sessionId));
        await using DbDataReader reader = await command
            .ExecuteReaderAsync(cancellationToken);

        if (await reader.ReadAsync(cancellationToken) is false)
        {
            return null;
        }

        return new UserSession(
            reader.GetGuid(0),
            AccountId.Create(reader.GetInt64(1)));
    }
}