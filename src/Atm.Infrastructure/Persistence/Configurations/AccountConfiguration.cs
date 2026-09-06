using Atm.Domain.Accounts;
using Atm.Domain.Users;
using Atm.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Atm.Infrastructure.Persistence.Configurations;

public sealed class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.ToTable("accounts", table =>
        {
            table.HasCheckConstraint("ck_accounts_id", ColumnRules.NonEmptyUuid("id"));
            table.HasCheckConstraint("ck_accounts_balance", ColumnRules.MonetaryCheck("balance"));
            table.HasCheckConstraint("ck_accounts_number", "length(btrim(number)) > 0 AND length(normalized_number) > 0");
            table.HasCheckConstraint("ck_accounts_pin_hash", "length(btrim(pin_hash)) > 0");
            table.HasCheckConstraint("ck_accounts_version", "version >= 0");
            table.HasCheckConstraint("ck_accounts_timestamps", "updated_at_utc >= created_at_utc");
        });
        builder.HasKey(account => account.Id).HasName("pk_accounts");
        builder.Property(account => account.Id).HasColumnName("id").HasColumnType("uuid").ValueGeneratedNever();
        builder.Property(account => account.OwnerUserId).HasColumnName("owner_user_id").HasColumnType("uuid");
        ColumnRules.Money(builder.Property(account => account.Balance), "balance");
        builder.Property(account => account.PinHash).HasColumnName("pin_hash")
            .HasConversion(value => value.Value, value => new PinHash(value)).HasMaxLength(512).IsRequired();
        builder.Property(account => account.Version).HasColumnName("version").HasColumnType("bigint")
            .IsConcurrencyToken().ValueGeneratedNever();
        builder.Property(account => account.CreatedAtUtc).HasColumnName("created_at_utc").HasColumnType("timestamp with time zone");
        builder.Property(account => account.UpdatedAtUtc).HasColumnName("updated_at_utc").HasColumnType("timestamp with time zone");
        builder.HasOne<User>().WithMany().HasForeignKey(account => account.OwnerUserId)
            .OnDelete(DeleteBehavior.Restrict).HasConstraintName("fk_accounts_users_owner_user_id");
        builder.HasIndex(account => account.OwnerUserId).HasDatabaseName("ix_accounts_owner_user_id");
        builder.OwnsOne(account => account.Number, number =>
        {
            number.WithOwner().HasForeignKey("AccountId");
            number.Property<Guid>("AccountId").HasColumnName("id").ValueGeneratedNever();
            number.HasKey("AccountId").HasName("pk_accounts");
            number.Property(value => value.Value).HasColumnName("number").HasMaxLength(64).IsRequired();
            number.Property(value => value.NormalizedValue).HasColumnName("normalized_number").HasMaxLength(64).UseCollation("C").IsRequired();
            number.Property<long>("Version").HasColumnName("version").IsConcurrencyToken().ValueGeneratedNever();
            number.HasIndex(value => value.NormalizedValue).IsUnique().HasDatabaseName("ux_accounts_normalized_number");
        });
        builder.Navigation(account => account.Number).IsRequired();
    }
}
