using Atm.Application.Abstractions.Repositories;
using Atm.Application.Models.Sessions;
using Npgsql;
using System.Data.Common;
using System.Runtime.CompilerServices;

namespace Atm.Infrastructure.Persistence.Repositories;

internal class AdminSessionRepository : IAdminSessionRepository
{
    private readonly NpgsqlDataSource _dataSource;

    public AdminSessionRepository(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public async IAsyncEnumerable<AdminSession> AddSessionAsync(
        IReadOnlyCollection<AdminSession> sessions,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        if (sessions.Count == 0)
        {
            yield break;
        }

        await using NpgsqlConnection connection = await _dataSource
            .OpenConnectionAsync(cancellationToken);

        const string sql = """
                           INSERT INTO admin_sessions (admin_session_id)
                           SELECT *
                           FROM UNNEST(:session_ids)
                           RETURNING admin_session_id;
                           """;

        Guid[] ids = [.. sessions.Select(x => x.SessionId)];

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.Add(new NpgsqlParameter("session_ids", ids));

        await using DbDataReader reader = await command
            .ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            yield return new AdminSession(
                reader.GetGuid(0));
        }
    }

    public async Task<AdminSession?> FindAdminSessionByIdAsync(
        Guid sessionId,
        CancellationToken cancellationToken)
    {
        await using NpgsqlConnection connection = await _dataSource
            .OpenConnectionAsync(cancellationToken);

        const string sql = """
                           SELECT admin_session_id
                           FROM admin_sessions
                           WHERE admin_session_id = :session_id;
                           """;

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.Add(new NpgsqlParameter("session_id", sessionId));
        await using DbDataReader reader = await command
            .ExecuteReaderAsync(cancellationToken);

        if (await reader.ReadAsync(cancellationToken) is false)
        {
            return null;
        }

        return new AdminSession(
            reader.GetGuid(0));
    }
}