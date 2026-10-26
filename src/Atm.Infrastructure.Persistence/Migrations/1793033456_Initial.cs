using FluentMigrator;

namespace Atm.Infrastructure.Persistence.Migrations;

[Migration(179303343456, "Initial")]
public sealed class Initial : Migration
{
    public override void Up()
    {
        Create.Table("accounts")
            .WithColumn("account_id").AsInt64().NotNullable().PrimaryKey().Identity()
            .WithColumn("account_number").AsString().NotNullable().Unique()
            .WithColumn("pincode").AsString().NotNullable()
            .WithColumn("balance").AsDecimal().NotNullable();

        Create.Table("operation_history")
            .WithColumn("operation_id").AsInt64().NotNullable().PrimaryKey().Identity()
            .WithColumn("account_id").AsInt64().NotNullable()
            .WithColumn("time").AsDateTimeOffset().NotNullable()
            .WithColumn("operation_type").AsInt32().NotNullable()
            .WithColumn("amount").AsDecimal().NotNullable()
            .WithColumn("balance_after").AsDecimal().NotNullable();

        Create.Table("admin_sessions")
            .WithColumn("admin_session_id").AsGuid().NotNullable().PrimaryKey();

        Create.Table("user_sessions")
            .WithColumn("user_session_id").AsGuid().NotNullable().PrimaryKey()
            .WithColumn("account_id").AsInt64().NotNullable();

        Create.ForeignKey("fk_operation_history_accounts")
            .FromTable("operation_history").ForeignColumn("account_id")
            .ToTable("accounts").PrimaryColumn("account_id");

        Create.ForeignKey("fk_user_sessions_accounts")
            .FromTable("user_sessions").ForeignColumn("account_id")
            .ToTable("accounts").PrimaryColumn("account_id");

        Execute.Sql("""
                    ALTER TABLE accounts
                    ADD CONSTRAINT ck_accounts_balance_non_negative
                    CHECK (balance >= 0),
                    ADD CONSTRAINT ck_accounts_account_number_format
                    CHECK (account_number ~ '^[0-9]{20}$'),
                    ADD CONSTRAINT ck_accounts_pincode_format
                    CHECK (pincode ~ '^[0-9]{4}$');
                    """);

        Execute.Sql("""
                    ALTER TABLE operation_history
                    ADD CONSTRAINT ck_operation_history_type
                    CHECK (operation_type IN (0, 1, 2)),
                    ADD CONSTRAINT ck_operation_history_amount
                    CHECK (
                        (operation_type IN (0, 1) AND amount > 0) 
                        OR
                        (operation_type = 2 AND amount = 0)
                    ),
                    ADD CONSTRAINT ck_operation_history_balance_after_non_negative
                    CHECK (balance_after >= 0);
                    """);
    }

    public override void Down()
    {
        Delete.Table("operation_history").IfExists();
        Delete.Table("admin_sessions").IfExists();
        Delete.Table("user_sessions").IfExists();
        Delete.Table("accounts").IfExists();
    }
}