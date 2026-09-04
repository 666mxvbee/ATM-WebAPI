using Atm.Domain.Operations.Results;
using Atm.Domain.ValueObjects;

namespace Atm.Domain.Operations;

public sealed class AccountHistoryEntry
{
    private AccountHistoryEntry() { }

    public Guid Id { get; private set; }

    public Guid TransactionId { get; private set; }

    public Guid AccountId { get; private set; }

    public AccountHistoryEntryType EntryType { get; private set; }

    public Money BalanceBefore { get; private set; } = null!;

    public Money BalanceAfter { get; private set; } = null!;

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public static AccountHistoryEntryCreationResult Create(
        Guid id,
        BankTransaction? transaction,
        Guid accountId,
        Money? balanceBefore)
    {
        if (id == Guid.Empty || accountId == Guid.Empty)
            return new AccountHistoryEntryCreationResult.Failure("History entry and account IDs must not be empty.");
        if (transaction is null || balanceBefore is null)
            return new AccountHistoryEntryCreationResult.Failure("Transaction and previous balance are required.");

        AccountHistoryEntryType entryType;
        Money balanceAfter;
        try
        {
            if (transaction.Type == BankTransactionType.Deposit && transaction.TargetAccountId == accountId)
            {
                entryType = AccountHistoryEntryType.Deposit;
                balanceAfter = balanceBefore.Add(transaction.GetDepositCreditAmount());
            }
            else if (transaction.Type == BankTransactionType.Withdrawal && transaction.SourceAccountId == accountId)
            {
                entryType = AccountHistoryEntryType.Withdrawal;
                balanceAfter = balanceBefore.Subtract(transaction.GetSourceDebitAmount());
            }
            else if (transaction.Type == BankTransactionType.Transfer && transaction.SourceAccountId == accountId)
            {
                entryType = AccountHistoryEntryType.TransferDebit;
                balanceAfter = balanceBefore.Subtract(transaction.GetSourceDebitAmount());
            }
            else if (transaction.Type == BankTransactionType.Transfer && transaction.TargetAccountId == accountId)
            {
                entryType = AccountHistoryEntryType.TransferCredit;
                balanceAfter = balanceBefore.Add(transaction.Amount);
            }
            else
            {
                return new AccountHistoryEntryCreationResult.Failure("Account does not participate in the transaction.");
            }
        }
        catch (InvalidOperationException exception)
        {
            return new AccountHistoryEntryCreationResult.Failure(exception.Message);
        }
        catch (OverflowException)
        {
            return new AccountHistoryEntryCreationResult.Failure("Resulting balance cannot be represented exactly as decimal.");
        }

        return new AccountHistoryEntryCreationResult.Success(new AccountHistoryEntry
        {
            Id = id,
            TransactionId = transaction.Id,
            AccountId = accountId,
            EntryType = entryType,
            BalanceBefore = balanceBefore,
            BalanceAfter = balanceAfter,
            CreatedAtUtc = transaction.CreatedAtUtc,
        });
    }
}
