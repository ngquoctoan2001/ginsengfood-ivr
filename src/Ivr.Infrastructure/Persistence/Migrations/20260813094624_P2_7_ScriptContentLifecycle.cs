using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1062, CA1707, CA1861, IDE0161 // EF-generated migration shape.

namespace Ivr.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class P2_7_ScriptContentLifecycle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ivr_script_versions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    template_id = table.Column<string>(type: "text", nullable: false),
                    version = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    template_text = table.Column<string>(type: "text", nullable: false),
                    template_hash = table.Column<string>(type: "text", nullable: false),
                    allowed_input_fields_json = table.Column<string>(type: "jsonb", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: false),
                    create_reason = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    submitted_by = table.Column<string>(type: "text", nullable: true),
                    submit_reason = table.Column<string>(type: "text", nullable: true),
                    submitted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    retired_by = table.Column<string>(type: "text", nullable: true),
                    retire_reason = table.Column<string>(type: "text", nullable: true),
                    retired_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ivr_script_versions", x => x.id);
                    table.CheckConstraint("ck_ivr_script_versions_hash", "template_hash ~ '^[a-f0-9]{64}$'");
                    table.CheckConstraint("ck_ivr_script_versions_status", "status IN ('DRAFT','IN_REVIEW','APPROVED','RETIRED')");
                });

            migrationBuilder.CreateTable(
                name: "ivr_script_approvals",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    script_version_id = table.Column<Guid>(type: "uuid", nullable: false),
                    approval_type = table.Column<string>(type: "text", nullable: false),
                    actor_id = table.Column<string>(type: "text", nullable: false),
                    reason = table.Column<string>(type: "text", nullable: false),
                    correlation_id = table.Column<string>(type: "text", nullable: false),
                    approved_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ivr_script_approvals", x => x.id);
                    table.CheckConstraint("ck_ivr_script_approvals_type", "approval_type IN ('MOCK_TEST','LAB','CONTENT','PRIVACY_LEGAL')");
                    table.ForeignKey(
                        name: "FK_ivr_script_approvals_ivr_script_versions_script_version_id",
                        column: x => x.script_version_id,
                        principalTable: "ivr_script_versions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ivr_script_approvals_approved_at",
                table: "ivr_script_approvals",
                column: "approved_at");

            migrationBuilder.CreateIndex(
                name: "IX_ivr_script_approvals_script_version_id_approval_type",
                table: "ivr_script_approvals",
                columns: new[] { "script_version_id", "approval_type" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ivr_script_versions_created_at",
                table: "ivr_script_versions",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_ivr_script_versions_status",
                table: "ivr_script_versions",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_ivr_script_versions_template_id_version",
                table: "ivr_script_versions",
                columns: new[] { "template_id", "version" },
                unique: true);

            migrationBuilder.InsertData(
                table: "ivr_script_versions",
                columns: new[]
                {
                    "id", "template_id", "version", "status", "template_text", "template_hash",
                    "allowed_input_fields_json", "created_by", "create_reason", "created_at",
                    "submitted_by", "submit_reason", "submitted_at"
                },
                values: new object[]
                {
                    new Guid("27000000-0000-0000-0000-000000000001"),
                    "SCRIPT-ORDER-CONFIRM",
                    "v1-test-approved",
                    "APPROVED",
                    "Xin chào anh/chị {{customer_display_name}}. Anh/chị có đơn {{order_code_short}} gồm {{items_spoken}}, tổng tiền {{total_amount_display}}, giao đến {{delivery_area_short}}. Bấm phím 1 để xác nhận đơn hàng, bấm phím 0 để hủy.",
                    "e228e0ac4fbfc0b5b37817739c83590f6cb14cddd73fbc27c6437e503ca780bd",
                    "[\"customer_display_name\",\"order_code_short\",\"items[].public_name\",\"items[].quantity\",\"items[].unit_label\",\"total_amount\",\"currency\",\"delivery_area_short\",\"program_display_name\",\"locale\",\"pronunciation_hints\"]",
                    "fixture-author",
                    "W-0024 synthetic MOCK fixture only",
                    new DateTimeOffset(2026, 8, 12, 0, 0, 0, TimeSpan.Zero),
                    "fixture-reviewer",
                    "W-0024 synthetic MOCK fixture review",
                    new DateTimeOffset(2026, 8, 12, 0, 0, 0, TimeSpan.Zero)
                });

            migrationBuilder.InsertData(
                table: "ivr_script_approvals",
                columns: new[]
                {
                    "id", "script_version_id", "approval_type", "actor_id", "reason",
                    "correlation_id", "approved_at"
                },
                values: new object[]
                {
                    new Guid("27000000-0000-0000-0000-000000000002"),
                    new Guid("27000000-0000-0000-0000-000000000001"),
                    "MOCK_TEST",
                    "fixture-reviewer",
                    "W-0024 synthetic MOCK fixture only",
                    "W-0024-SEED",
                    new DateTimeOffset(2026, 8, 12, 0, 0, 0, TimeSpan.Zero)
                });

            migrationBuilder.Sql(
                """
                CREATE FUNCTION ivr_enforce_script_version_lifecycle()
                RETURNS trigger
                LANGUAGE plpgsql
                AS $function$
                BEGIN
                    IF TG_OP = 'DELETE' THEN
                        RAISE EXCEPTION 'script versions are retired, never deleted';
                    END IF;

                    IF NEW.template_id IS DISTINCT FROM OLD.template_id
                       OR NEW.version IS DISTINCT FROM OLD.version
                       OR NEW.template_text IS DISTINCT FROM OLD.template_text
                       OR NEW.template_hash IS DISTINCT FROM OLD.template_hash
                       OR NEW.allowed_input_fields_json IS DISTINCT FROM OLD.allowed_input_fields_json
                       OR NEW.created_by IS DISTINCT FROM OLD.created_by
                       OR NEW.create_reason IS DISTINCT FROM OLD.create_reason
                       OR NEW.created_at IS DISTINCT FROM OLD.created_at
                       OR NEW.submitted_by IS DISTINCT FROM OLD.submitted_by
                       OR NEW.submit_reason IS DISTINCT FROM OLD.submit_reason
                       OR NEW.submitted_at IS DISTINCT FROM OLD.submitted_at THEN
                        IF OLD.status IN ('APPROVED','RETIRED')
                           OR NEW.status IN ('APPROVED','RETIRED')
                           OR EXISTS (
                               SELECT 1
                               FROM ivr_script_approvals approval
                               WHERE approval.script_version_id = OLD.id) THEN
                            RAISE EXCEPTION 'approved script content is immutable';
                        END IF;
                    END IF;

                    IF OLD.status = 'RETIRED' THEN
                        RAISE EXCEPTION 'retired script versions are immutable';
                    END IF;

                    IF OLD.status = 'DRAFT' AND NEW.status NOT IN ('DRAFT','IN_REVIEW') THEN
                        RAISE EXCEPTION 'invalid script lifecycle transition';
                    ELSIF OLD.status = 'IN_REVIEW' AND NEW.status NOT IN ('IN_REVIEW','APPROVED','RETIRED') THEN
                        RAISE EXCEPTION 'invalid script lifecycle transition';
                    ELSIF OLD.status = 'APPROVED' AND NEW.status NOT IN ('APPROVED','RETIRED') THEN
                        RAISE EXCEPTION 'invalid script lifecycle transition';
                    END IF;

                    IF NEW.status = 'RETIRED'
                       AND (NEW.retired_by IS NULL OR NEW.retire_reason IS NULL OR NEW.retired_at IS NULL) THEN
                        RAISE EXCEPTION 'script retirement requires actor, reason and timestamp';
                    ELSIF NEW.status <> 'RETIRED'
                          AND (NEW.retired_by IS NOT NULL OR NEW.retire_reason IS NOT NULL OR NEW.retired_at IS NOT NULL) THEN
                        RAISE EXCEPTION 'script retirement metadata requires retired status';
                    END IF;

                    RETURN NEW;
                END;
                $function$;

                CREATE TRIGGER trg_ivr_script_version_lifecycle
                BEFORE UPDATE OR DELETE ON ivr_script_versions
                FOR EACH ROW EXECUTE FUNCTION ivr_enforce_script_version_lifecycle();

                CREATE FUNCTION ivr_validate_script_approval_insert()
                RETURNS trigger
                LANGUAGE plpgsql
                AS $function$
                DECLARE
                    parent_status text;
                    parent_creator text;
                BEGIN
                    SELECT status, created_by
                    INTO parent_status, parent_creator
                    FROM ivr_script_versions
                    WHERE id = NEW.script_version_id;

                    IF parent_status NOT IN ('IN_REVIEW','APPROVED') THEN
                        RAISE EXCEPTION 'script approval requires reviewed version';
                    END IF;

                    IF parent_creator = NEW.actor_id THEN
                        RAISE EXCEPTION 'script creator cannot approve same version';
                    END IF;

                    IF NEW.approval_type IN ('CONTENT','PRIVACY_LEGAL')
                       AND EXISTS (
                           SELECT 1
                           FROM ivr_script_approvals approval
                           WHERE approval.script_version_id = NEW.script_version_id
                             AND approval.approval_type IN ('CONTENT','PRIVACY_LEGAL')
                             AND approval.actor_id = NEW.actor_id) THEN
                        RAISE EXCEPTION 'content and privacy legal approvals require different actors';
                    END IF;

                    RETURN NEW;
                END;
                $function$;

                CREATE TRIGGER trg_ivr_script_approval_validate_insert
                BEFORE INSERT ON ivr_script_approvals
                FOR EACH ROW EXECUTE FUNCTION ivr_validate_script_approval_insert();

                CREATE FUNCTION ivr_enforce_script_approval_append_only()
                RETURNS trigger
                LANGUAGE plpgsql
                AS $function$
                BEGIN
                    RAISE EXCEPTION 'script approvals are append-only';
                END;
                $function$;

                CREATE TRIGGER trg_ivr_script_approval_append_only
                BEFORE UPDATE OR DELETE ON ivr_script_approvals
                FOR EACH ROW EXECUTE FUNCTION ivr_enforce_script_approval_append_only();
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DROP TRIGGER IF EXISTS trg_ivr_script_approval_append_only ON ivr_script_approvals;
                DROP FUNCTION IF EXISTS ivr_enforce_script_approval_append_only();
                DROP TRIGGER IF EXISTS trg_ivr_script_approval_validate_insert ON ivr_script_approvals;
                DROP FUNCTION IF EXISTS ivr_validate_script_approval_insert();
                DROP TRIGGER IF EXISTS trg_ivr_script_version_lifecycle ON ivr_script_versions;
                DROP FUNCTION IF EXISTS ivr_enforce_script_version_lifecycle();
                """);

            migrationBuilder.DropTable(
                name: "ivr_script_approvals");

            migrationBuilder.DropTable(
                name: "ivr_script_versions");
        }
    }
}
