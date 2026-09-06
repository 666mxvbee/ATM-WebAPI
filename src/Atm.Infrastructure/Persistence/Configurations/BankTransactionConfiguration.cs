using Atm.Domain.Accounts;
using Atm.Domain.Operations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Atm.Infrastructure.Persistence.Configurations;

public sealed class BankTransactionConfiguration : IEntityTypeConfiguration<BankTransaction>
{
    public void Configure(EntityTypeBuilder<BankTransaction> builder)
    {
        builder.ToTable("bank_transactions", table =>
        {
            table.HasCheckConstraint("ck_bank_transactions_id", ColumnRules.NonEmptyUuid("id"));
            table.HasCheckConstraint("ck_bank_transactions_request_id", ColumnRules.NonEmptyUuid("request_id"));
            table.HasCheckConstraint("ck_bank_transactions_amount", ColumnRules.MonetaryCheck("amount", true));
            table.HasCheckConstraint("ck_bank_transactions_commission_rate", "commission_rate BETWEEN 0 AND 1");
            table.HasCheckConstraint("ck_bank_transactions_commission_fixed", ColumnRules.MonetaryCheck("commission_fixed"));
            table.HasCheckConstraint("ck_bank_transactions_commission_amount", ColumnRules.MonetaryCheck("commission_amount"));
            table.HasCheckConstraint("ck_bank_transactions_account_shape",
                "(type = 'Deposit' AND source_account_id IS NULL AND target_account_id IS NOT NULL) OR "
                + "(type = 'Withdrawal' AND source_account_id IS NOT NULL AND target_account_id IS NULL) OR "
                + "(type = 'Transfer' AND source_account_id IS NOT NULL AND target_account_id IS NOT NULL AND source_account_id <> target_account_id)");
            table.HasCheckConstraint("ck_bank_transactions_deposit_credit", "type <> 'Deposit' OR amount > commission_amount");
        });
        builder.HasKey(transaction => transaction.Id).HasName("pk_bank_transactions");
        builder.Property(transaction => transaction.Id).HasColumnName("id").HasColumnType("uuid").ValueGeneratedNever();
        builder.Property(transaction => transaction.RequestId).HasColumnName("request_id").HasColumnType("uuid").ValueGeneratedNever();
        builder.Property(transaction => transaction.Type).HasColumnName("type").HasConversion<string>().HasMaxLength(32);
        builder.Property(transaction => transaction.SourceAccountId).HasColumnName("source_account_id").HasColumnType("uuid");
        builder.Property(transaction => transaction.TargetAccountId).HasColumnName("target_account_id").HasColumnType("uuid");
        ColumnRules.Amount(builder.Property(transaction => transaction.Amount), "amount");
        builder.Property(transaction => transaction.CreatedAtUtc).HasColumnName("created_at_utc").HasColumnType("timestamp with time zone");
        builder.HasIndex(transaction => transaction.RequestId).IsUnique().HasDatabaseName("ux_bank_transactions_request_id");
        builder.HasIndex(transaction => transaction.SourceAccountId).HasDatabaseName("ix_bank_transactions_source_account_id");
        builder.HasIndex(transaction => transaction.TargetAccountId).HasDatabaseName("ix_bank_transactions_target_account_id");
        builder.HasOne<Account>().WithMany().HasForeignKey(transaction => transaction.SourceAccountId)
            .OnDelete(DeleteBehavior.Restrict).HasConstraintName("fk_bank_transactions_accounts_source_account_id");
        builder.HasOne<Account>().WithMany().HasForeignKey(transaction => transaction.TargetAccountId)
            .OnDelete(DeleteBehavior.Restrict).HasConstraintName("fk_bank_transactions_accounts_target_account_id");
        builder.OwnsOne(transaction => transaction.Commission, commission =>
        {
            commission.WithOwner().HasForeignKey("TransactionId");
            commission.Property<Guid>("TransactionId").HasColumnName("id").ValueGeneratedNever();
            commission.HasKey("TransactionId").HasName("pk_bank_transactions");
            commission.Property(value => value.Rate).HasColumnName("commission_rate").HasPrecision(9, 6);
            ColumnRules.Money(commission.Property(value => value.FixedAmount), "commission_fixed");
            ColumnRules.Money(commission.Property(value => value.CommissionAmount), "commission_amount");
        });
        builder.Navigation(transaction => transaction.Commission).IsRequired();
    }
}
