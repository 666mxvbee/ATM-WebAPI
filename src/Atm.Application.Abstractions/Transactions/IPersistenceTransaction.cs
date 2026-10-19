namespace Atm.Application.Abstractions.Transactions;

public interface IPersistenceTransaction : IAsyncDisposable
{
    ValueTask CommitAsync(CancellationToken cancellationToken);

    ValueTask RollbackAsync(CancellationToken cancellationToken);
}