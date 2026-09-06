using Atm.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Atm.Infrastructure.Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users", table =>
        {
            table.HasCheckConstraint("ck_users_id", ColumnRules.NonEmptyUuid("id"));
            table.HasCheckConstraint("ck_users_name", "length(btrim(name)) > 0");
            table.HasCheckConstraint("ck_users_login", "length(btrim(login)) > 0 AND length(normalized_login) > 0");
            table.HasCheckConstraint("ck_users_age", "age BETWEEN 0 AND 150");
            table.HasCheckConstraint("ck_users_gender", "gender IN ('Unspecified', 'Male', 'Female', 'Other')");
            table.HasCheckConstraint("ck_users_hair_color", "hair_color IN ('Unspecified', 'Black', 'Brown', 'Blond', 'Red', 'Gray', 'White', 'Other')");
            table.HasCheckConstraint("ck_users_timestamps", "updated_at_utc >= created_at_utc");
        });
        builder.HasKey(user => user.Id).HasName("pk_users");
        builder.Property(user => user.Id).HasColumnName("id").HasColumnType("uuid").ValueGeneratedNever();
        builder.Property(user => user.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
        builder.Property(user => user.Age).HasColumnName("age").HasColumnType("smallint");
        builder.Property(user => user.Gender).HasColumnName("gender").HasConversion<string>().HasMaxLength(32);
        builder.Property(user => user.HairColor).HasColumnName("hair_color").HasConversion<string>().HasMaxLength(32);
        builder.Property(user => user.CreatedAtUtc).HasColumnName("created_at_utc").HasColumnType("timestamp with time zone");
        builder.Property(user => user.UpdatedAtUtc).HasColumnName("updated_at_utc").HasColumnType("timestamp with time zone");
        builder.OwnsOne(user => user.Login, login =>
        {
            login.WithOwner().HasForeignKey("UserId");
            login.Property<Guid>("UserId").HasColumnName("id").ValueGeneratedNever();
            login.HasKey("UserId").HasName("pk_users");
            login.Property(value => value.Value).HasColumnName("login").HasMaxLength(64).IsRequired();
            login.Property(value => value.NormalizedValue).HasColumnName("normalized_login").HasMaxLength(64).UseCollation("C").IsRequired();
            login.HasIndex(value => value.NormalizedValue).IsUnique().HasDatabaseName("ux_users_normalized_login");
        });
        builder.Navigation(user => user.Login).IsRequired();
    }
}
