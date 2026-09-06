using Atm.Domain.Accounts;
using Atm.Domain.Operations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Atm.Infrastructure.Persistence.Configurations;

public sealed class AccountHistoryEntryConfiguration : IEntityTypeConfiguration<AccountHistoryEntry>
{
    public void Configure(EntityTypeBuilder<AccountHistoryEntry> builder)
    {
        builder.ToTable("account_history_entries", table =>
        {
            table.HasCheckConstraint("ck_account_history_entries_id", ColumnRules.NonEmptyUuid("id"));
            table.HasCheckConstraint("ck_account_history_entries_balance_before", ColumnRules.MonetaryCheck("balance_before"));
            table.HasCheckConstraint("ck_account_history_entries_balance_after", ColumnRules.MonetaryCheck("balance_after"));
            table.HasCheckConstraint("ck_account_history_entries_type",
                "entry_type IN ('Deposit', 'Withdrawal', 'TransferDebit', 'TransferCredit')");
            table.HasCheckConstraint("ck_account_history_entries_direction",
                "(entry_type IN ('Deposit', 'TransferCredit') AND balance_after > balance_before) OR "
                + "(entry_type IN ('Withdrawal', 'TransferDebit') AND balance_after < balance_before)");
        });
        builder.HasKey(entry => entry.Id).HasName("pk_account_history_entries");
        builder.Property(entry => entry.Id).HasColumnName("id").HasColumnType("uuid").ValueGeneratedNever();
        builder.Property(entry => entry.TransactionId).HasColumnName("transaction_id").HasColumnType("uuid");
        builder.Property(entry => entry.AccountId).HasColumnName("account_id").HasColumnType("uuid");
        builder.Property(entry => entry.EntryType).HasColumnName("entry_type").HasConversion<string>().HasMaxLength(32);
        ColumnRules.Money(builder.Property(entry => entry.BalanceBefore), "balance_before");
        ColumnRules.Money(builder.Property(entry => entry.BalanceAfter), "balance_after");
        builder.Property(entry => entry.CreatedAtUtc).HasColumnName("created_at_utc").HasColumnType("timestamp with time zone");
        builder.HasOne<Account>().WithMany().HasForeignKey(entry => entry.AccountId)
            .OnDelete(DeleteBehavior.Restrict).HasConstraintName("fk_account_history_entries_accounts_account_id");
        builder.HasOne<BankTransaction>().WithMany().HasForeignKey(entry => entry.TransactionId)
            .OnDelete(DeleteBehavior.Restrict).HasConstraintName("fk_account_history_entries_bank_transactions_transaction_id");
        builder.HasIndex(entry => new { entry.AccountId, entry.CreatedAtUtc, entry.Id })
            .IsDescending(false, true, true).HasDatabaseName("ix_account_history_entries_account_timeline");
        builder.HasIndex(entry => entry.TransactionId).HasDatabaseName("ix_account_history_entries_transaction_id");
        builder.HasAlternateKey(entry => new { entry.AccountId, entry.TransactionId, entry.EntryType })
            .HasName("ak_account_history_entries_account_transaction_type");
    }
}
