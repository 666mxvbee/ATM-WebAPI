using Atm.Domain.Accounts;
using Atm.Domain.Sessions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Atm.Infrastructure.Persistence.Configurations;

public sealed class SessionConfiguration : IEntityTypeConfiguration<Session>
{
    public void Configure(EntityTypeBuilder<Session> builder)
    {
        builder.ToTable("sessions", table =>
        {
            table.HasCheckConstraint("ck_sessions_id", ColumnRules.NonEmptyUuid("id"));
            table.HasCheckConstraint("ck_sessions_account_type",
                "(type = 'User' AND account_id IS NOT NULL) OR (type = 'Admin' AND account_id IS NULL)");
            table.HasCheckConstraint("ck_sessions_expiration", "expires_at_utc > created_at_utc");
            table.HasCheckConstraint("ck_sessions_revocation", "revoked_at_utc IS NULL OR revoked_at_utc >= created_at_utc");
        });
        builder.HasKey(session => session.Id).HasName("pk_sessions");
        builder.Property(session => session.Id).HasColumnName("id").HasColumnType("uuid").ValueGeneratedNever();
        builder.Property(session => session.Type).HasColumnName("type").HasConversion<string>().HasMaxLength(16);
        builder.Property(session => session.AccountId).HasColumnName("account_id").HasColumnType("uuid");
        builder.Property(session => session.CreatedAtUtc).HasColumnName("created_at_utc").HasColumnType("timestamp with time zone");
        builder.Property(session => session.ExpiresAtUtc).HasColumnName("expires_at_utc").HasColumnType("timestamp with time zone");
        builder.Property(session => session.RevokedAtUtc).HasColumnName("revoked_at_utc").HasColumnType("timestamp with time zone");
        builder.HasOne<Account>().WithMany().HasForeignKey(session => session.AccountId)
            .OnDelete(DeleteBehavior.Restrict).HasConstraintName("fk_sessions_accounts_account_id");
        builder.HasIndex(session => session.AccountId).HasDatabaseName("ix_sessions_account_id");
        builder.HasIndex(session => session.ExpiresAtUtc).HasDatabaseName("ix_sessions_expires_at_utc");
    }
}
