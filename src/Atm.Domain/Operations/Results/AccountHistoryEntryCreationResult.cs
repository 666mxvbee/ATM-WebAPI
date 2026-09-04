namespace Atm.Domain.Operations.Results;

public abstract record AccountHistoryEntryCreationResult
{
    private AccountHistoryEntryCreationResult() { }

    public sealed record Success(AccountHistoryEntry Entry) : AccountHistoryEntryCreationResult;

    public sealed record Failure(string ErrorMessage) : AccountHistoryEntryCreationResult;
}
