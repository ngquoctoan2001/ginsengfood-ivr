using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ivr.Infrastructure.Persistence.Migrations;

/// <summary>
/// W-0122. Drops the console account tables.
/// <para>
/// IVR no longer holds human identities. Module 3 owns the operator console and its accounts, and
/// reaches IVR as a service across three credential tiers, so nothing writes these rows any more.
/// </para>
/// <para>
/// <c>Down</c> recreates the tables empty. It restores the shape, not the accounts — the rows are
/// gone for good, which is the intended outcome of retiring an authentication system rather than
/// pausing it.
/// </para>
/// </summary>
public partial class W0122DropConsoleAccounts : Migration
{
    private static readonly string[] StatusRoleColumns = ["status", "role"];

    private static readonly string[] AccountExpiryColumns = ["account_id", "expires_at"];

    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        System.ArgumentNullException.ThrowIfNull(migrationBuilder);
        migrationBuilder.DropTable(
            name: "ivr_console_sessions");

        migrationBuilder.DropTable(
            name: "ivr_console_accounts");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        System.ArgumentNullException.ThrowIfNull(migrationBuilder);
        migrationBuilder.CreateTable(
            name: "ivr_console_accounts",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                anonymized_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                display_name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                failed_login_count = table.Column<int>(type: "integer", nullable: false),
                is_builtin = table.Column<bool>(type: "boolean", nullable: false),
                last_login_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                legal_hold_until = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                locked_until = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                password_changed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                password_hash = table.Column<string>(type: "text", nullable: false),
                retain_until = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                retention_class = table.Column<string>(type: "text", nullable: false),
                role = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                status = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                username = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                version = table.Column<long>(type: "bigint", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ivr_console_accounts", x => x.id);
                table.CheckConstraint("ck_ivr_console_accounts_builtin", "is_builtin IS FALSE OR (username = 'admin' AND role = 'Admin' AND status = 'ACTIVE')");
                table.CheckConstraint("ck_ivr_console_accounts_failed_login_count", "failed_login_count BETWEEN 0 AND 100");
                table.CheckConstraint("ck_ivr_console_accounts_role", "role IN ('Admin','Operator')");
                table.CheckConstraint("ck_ivr_console_accounts_status", "status IN ('ACTIVE','DISABLED','DELETED')");
                table.CheckConstraint("ck_ivr_console_accounts_username", "username ~ '^[a-z][a-z0-9._-]{2,63}$'");
            });

        migrationBuilder.CreateTable(
            name: "ivr_console_sessions",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                account_id = table.Column<Guid>(type: "uuid", nullable: false),
                anonymized_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                legal_hold_until = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                retain_until = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                retention_class = table.Column<string>(type: "text", nullable: false),
                revoke_reason = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                revoked_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                token_hash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ivr_console_sessions", x => x.id);
                table.CheckConstraint("ck_ivr_console_sessions_expiry", "expires_at > created_at");
                table.CheckConstraint("ck_ivr_console_sessions_revocation", "(revoked_at IS NULL AND revoke_reason IS NULL) OR (revoked_at IS NOT NULL AND revoke_reason IS NOT NULL)");
                table.CheckConstraint("ck_ivr_console_sessions_token_hash", "token_hash ~ '^[a-f0-9]{64}$'");
                table.ForeignKey(
                    name: "FK_ivr_console_sessions_ivr_console_accounts_account_id",
                    column: x => x.account_id,
                    principalTable: "ivr_console_accounts",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "IX_ivr_console_accounts_anonymized_at",
            table: "ivr_console_accounts",
            column: "anonymized_at");

        migrationBuilder.CreateIndex(
            name: "IX_ivr_console_accounts_deleted_at",
            table: "ivr_console_accounts",
            column: "deleted_at");

        migrationBuilder.CreateIndex(
            name: "IX_ivr_console_accounts_legal_hold_until",
            table: "ivr_console_accounts",
            column: "legal_hold_until");

        migrationBuilder.CreateIndex(
            name: "IX_ivr_console_accounts_retain_until",
            table: "ivr_console_accounts",
            column: "retain_until");

        migrationBuilder.CreateIndex(
            name: "IX_ivr_console_accounts_status_role",
            table: "ivr_console_accounts",
            columns: StatusRoleColumns);

        migrationBuilder.CreateIndex(
            name: "IX_ivr_console_accounts_username",
            table: "ivr_console_accounts",
            column: "username",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_ivr_console_sessions_account_id_expires_at",
            table: "ivr_console_sessions",
            columns: AccountExpiryColumns);

        migrationBuilder.CreateIndex(
            name: "IX_ivr_console_sessions_anonymized_at",
            table: "ivr_console_sessions",
            column: "anonymized_at");

        migrationBuilder.CreateIndex(
            name: "IX_ivr_console_sessions_legal_hold_until",
            table: "ivr_console_sessions",
            column: "legal_hold_until");

        migrationBuilder.CreateIndex(
            name: "IX_ivr_console_sessions_retain_until",
            table: "ivr_console_sessions",
            column: "retain_until");

        migrationBuilder.CreateIndex(
            name: "IX_ivr_console_sessions_revoked_at",
            table: "ivr_console_sessions",
            column: "revoked_at");

        migrationBuilder.CreateIndex(
            name: "IX_ivr_console_sessions_token_hash",
            table: "ivr_console_sessions",
            column: "token_hash",
            unique: true);
    }
}
