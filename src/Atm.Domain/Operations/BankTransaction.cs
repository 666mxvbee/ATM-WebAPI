using Atm.Domain.Common;
using Atm.Domain.Operations.Results;
using Atm.Domain.ValueObjects;

namespace Atm.Domain.Operations;

public sealed class BankTransaction
{
    private BankTransaction() { }

    public Guid Id { get; private set; }

    public Guid RequestId { get; private set; }

    public BankTransactionType Type { get; private set; }

    public Guid? SourceAccountId { get; private set; }

    public Guid? TargetAccountId { get; private set; }

    public Amount Amount { get; private set; } = null!;

    public CommissionSnapshot Commission { get; private set; } = null!;

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public static BankTransactionCreationResult Create(
        Guid id,
        Guid requestId,
        BankTransactionType type,
        Guid? sourceAccountId,
        Guid? targetAccountId,
        Amount? amount,
        CommissionSnapshot? commission,
        DateTimeOffset createdAtUtc)
    {
        if (id == Guid.Empty || requestId == Guid.Empty)
            return new BankTransactionCreationResult.Failure("Transaction and request IDs must not be empty.");
        if (!Enum.IsDefined(type))
            return new BankTransactionCreationResult.Failure("Transaction type is not supported.");
        if (amount is null || commission is null)
            return new BankTransactionCreationResult.Failure("Amount and commission snapshot are required.");
        if (!commission.IsCalculatedFor(amount))
            return new BankTransactionCreationResult.Failure("Commission snapshot does not match the transaction amount.");
        if (!DomainRules.IsUtc(createdAtUtc))
            return new BankTransactionCreationResult.Failure("Transaction timestamp must be UTC.");

        string? shapeError = ValidateAccountShape(type, sourceAccountId, targetAccountId);
        if (shapeError is not null)
            return new BankTransactionCreationResult.Failure(shapeError);

        try
        {
            if (type == BankTransactionType.Deposit)
                _ = CalculateDepositCredit(amount, commission);
            else
                _ = CalculateSourceDebit(amount, commission);
        }
        catch (InvalidOperationException exception)
        {
            return new BankTransactionCreationResult.Failure(exception.Message);
        }
        catch (OverflowException)
        {
            return new BankTransactionCreationResult.Failure("The transaction amount cannot be represented exactly as decimal.");
        }

        return new BankTransactionCreationResult.Success(new BankTransaction
        {
            Id = id,
            RequestId = requestId,
            Type = type,
            SourceAccountId = sourceAccountId,
            TargetAccountId = targetAccountId,
            Amount = amount,
            Commission = commission,
            CreatedAtUtc = createdAtUtc,
        });
    }

    public Amount GetDepositCreditAmount()
    {
        if (Type != BankTransactionType.Deposit)
            throw new InvalidOperationException("Only a deposit has a deposit credit amount.");
        return CalculateDepositCredit(Amount, Commission);
    }

    public Amount GetSourceDebitAmount()
    {
        if (Type == BankTransactionType.Deposit)
            throw new InvalidOperationException("A deposit has no source debit amount.");
        return CalculateSourceDebit(Amount, Commission);
    }

    private static string? ValidateAccountShape(
        BankTransactionType type,
        Guid? sourceAccountId,
        Guid? targetAccountId)
    {
        bool hasSource = sourceAccountId is Guid source && source != Guid.Empty;
        bool hasTarget = targetAccountId is Guid target && target != Guid.Empty;

        return type switch
        {
            BankTransactionType.Deposit when !hasSource && hasTarget => null,
            BankTransactionType.Withdrawal when hasSource && !hasTarget => null,
            BankTransactionType.Transfer when hasSource && hasTarget && sourceAccountId != targetAccountId => null,
            _ => "Source and target accounts do not match the transaction type.",
        };
    }

    private static Amount CalculateDepositCredit(Amount amount, CommissionSnapshot commission)
    {
        if (amount.Value <= commission.CommissionAmount.Value)
            throw new InvalidOperationException("Deposit amount must exceed its commission.");
        return new Amount(DomainRules.SubtractExact(amount.Value, commission.CommissionAmount.Value));
    }

    private static Amount CalculateSourceDebit(Amount amount, CommissionSnapshot commission)
    {
        return new Amount(DomainRules.AddExact(amount.Value, commission.CommissionAmount.Value));
    }
}
