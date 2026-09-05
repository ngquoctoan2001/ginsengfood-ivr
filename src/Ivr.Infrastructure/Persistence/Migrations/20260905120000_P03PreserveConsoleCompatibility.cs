using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ivr.Infrastructure.Persistence.Migrations;

/// <summary>
/// Repairs the compatibility shape where the old W0122 drop already ran; never recovers lost data.
/// Existing rows are untouched. The retired tables deliberately stay outside the runtime model.
/// Application rollback retains this additive schema; cleanup requires a later contract release.
/// </summary>
[DbContext(typeof(IvrDbContext))]
[Migration("20260905120000_P03PreserveConsoleCompatibility")]
public sealed class P03PreserveConsoleCompatibility : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        ArgumentNullException.ThrowIfNull(migrationBuilder);
        migrationBuilder.Sql(
            """
            CREATE TABLE IF NOT EXISTS ivr_console_accounts (
                id uuid CONSTRAINT "PK_ivr_console_accounts" PRIMARY KEY,
                anonymized_at timestamptz NULL,
                created_at timestamptz NOT NULL,
                deleted_at timestamptz NULL,
                display_name varchar(128) NOT NULL,
                failed_login_count integer NOT NULL,
                is_builtin boolean NOT NULL,
                last_login_at timestamptz NULL,
                legal_hold_until timestamptz NULL,
                locked_until timestamptz NULL,
                password_changed_at timestamptz NOT NULL,
                password_hash text NOT NULL,
                retain_until timestamptz NULL,
                retention_class text NOT NULL,
                role varchar(16) NOT NULL,
                status varchar(16) NOT NULL,
                updated_at timestamptz NOT NULL,
                username varchar(64) NOT NULL,
                version bigint NOT NULL,
                CONSTRAINT ck_ivr_console_accounts_builtin CHECK
                    (is_builtin IS FALSE OR (username = 'admin' AND role = 'Admin' AND status = 'ACTIVE')),
                CONSTRAINT ck_ivr_console_accounts_failed_login_count CHECK (failed_login_count BETWEEN 0 AND 100),
                CONSTRAINT ck_ivr_console_accounts_role CHECK (role IN ('Admin','Operator')),
                CONSTRAINT ck_ivr_console_accounts_status CHECK (status IN ('ACTIVE','DISABLED','DELETED')),
                CONSTRAINT ck_ivr_console_accounts_username CHECK (username ~ '^[a-z][a-z0-9._-]{2,63}$')
            );
            CREATE TABLE IF NOT EXISTS ivr_console_sessions (
                id uuid CONSTRAINT "PK_ivr_console_sessions" PRIMARY KEY,
                account_id uuid NOT NULL,
                anonymized_at timestamptz NULL,
                created_at timestamptz NOT NULL,
                expires_at timestamptz NOT NULL,
                legal_hold_until timestamptz NULL,
                retain_until timestamptz NULL,
                retention_class text NOT NULL,
                revoke_reason varchar(64) NULL,
                revoked_at timestamptz NULL,
                token_hash varchar(64) NOT NULL,
                CONSTRAINT ck_ivr_console_sessions_expiry CHECK (expires_at > created_at),
                CONSTRAINT ck_ivr_console_sessions_revocation CHECK
                    ((revoked_at IS NULL AND revoke_reason IS NULL) OR (revoked_at IS NOT NULL AND revoke_reason IS NOT NULL)),
                CONSTRAINT ck_ivr_console_sessions_token_hash CHECK (token_hash ~ '^[a-f0-9]{64}$'),
                CONSTRAINT "FK_ivr_console_sessions_ivr_console_accounts_account_id"
                    FOREIGN KEY (account_id) REFERENCES ivr_console_accounts(id) ON DELETE RESTRICT
            );
            CREATE INDEX IF NOT EXISTS "IX_ivr_console_accounts_anonymized_at" ON ivr_console_accounts (anonymized_at);
            CREATE INDEX IF NOT EXISTS "IX_ivr_console_accounts_deleted_at" ON ivr_console_accounts (deleted_at);
            CREATE INDEX IF NOT EXISTS "IX_ivr_console_accounts_legal_hold_until" ON ivr_console_accounts (legal_hold_until);
            CREATE INDEX IF NOT EXISTS "IX_ivr_console_accounts_retain_until" ON ivr_console_accounts (retain_until);
            CREATE INDEX IF NOT EXISTS "IX_ivr_console_accounts_status_role" ON ivr_console_accounts (status, role);
            CREATE UNIQUE INDEX IF NOT EXISTS "IX_ivr_console_accounts_username" ON ivr_console_accounts (username);
            CREATE INDEX IF NOT EXISTS "IX_ivr_console_sessions_account_id_expires_at" ON ivr_console_sessions (account_id, expires_at);
            CREATE INDEX IF NOT EXISTS "IX_ivr_console_sessions_anonymized_at" ON ivr_console_sessions (anonymized_at);
            CREATE INDEX IF NOT EXISTS "IX_ivr_console_sessions_legal_hold_until" ON ivr_console_sessions (legal_hold_until);
            CREATE INDEX IF NOT EXISTS "IX_ivr_console_sessions_retain_until" ON ivr_console_sessions (retain_until);
            CREATE INDEX IF NOT EXISTS "IX_ivr_console_sessions_revoked_at" ON ivr_console_sessions (revoked_at);
            CREATE UNIQUE INDEX IF NOT EXISTS "IX_ivr_console_sessions_token_hash" ON ivr_console_sessions (token_hash);
            """);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        ArgumentNullException.ThrowIfNull(migrationBuilder);
        // Data stays across rollback. Original W0105 still owns full teardown to an empty DB.
    }
}
