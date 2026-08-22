using Ivr.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1062, CA1707, CA1861 // EF-generated migration shape.

namespace Ivr.Infrastructure.Persistence.Migrations;

/// <summary>
/// Adds the owner-requested W-0104 MOCK script as a new immutable version. The historical v1
/// fixture remains available so already-created development tasks can still be rendered.
/// This is MOCK_TEST approval only; it does not grant lab, production, privacy or legal approval.
/// </summary>
[DbContext(typeof(IvrDbContext))]
[Migration("20260822095000_W0104ModernOrderConfirmationScript")]
public sealed class W0104ModernOrderConfirmationScript : Migration
{
    private static readonly Guid ScriptVersionId =
        new("10400000-0000-0000-0000-000000000001");

    private static readonly Guid ApprovalId =
        new("10400000-0000-0000-0000-000000000002");

    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Raw SQL keeps this data-only migration independent of a generated target-model snapshot.
        migrationBuilder.Sql(
            """
            INSERT INTO ivr_script_versions (
                id, template_id, version, status, template_text, template_hash,
                allowed_input_fields_json, created_by, create_reason, created_at,
                submitted_by, submit_reason, submitted_at)
            VALUES (
                '10400000-0000-0000-0000-000000000001',
                'SCRIPT-ORDER-CONFIRM',
                'v2-test-approved',
                'APPROVED',
                'Xin chào {{customer_display_name}}. Đây là cuộc gọi tự động để xác nhận đơn hàng từ Ginsengfood. Anh/chị có đơn hàng gồm {{items_spoken}}, tổng tiền {{total_amount_display}}, giao đến {{delivery_area_short}}. Bấm phím một để xác nhận đơn hàng, hoặc bấm phím không để hủy đơn hàng.',
                '3ab54eb7f9d9b5bb5c788c23722a970da0552e313a85a13587bb8cceb7d75532',
                '["customer_display_name","order_code_short","items[].public_name","items[].quantity","items[].unit_label","total_amount","currency","delivery_area_short","program_display_name","locale","pronunciation_hints"]',
                'owner-w0104',
                'W-0104 owner-requested modern MOCK script',
                TIMESTAMPTZ '2026-08-22 02:50:00+00',
                'reviewer-w0104',
                'W-0104 MOCK wording review',
                TIMESTAMPTZ '2026-08-22 02:50:00+00');

            INSERT INTO ivr_script_approvals (
                id, script_version_id, approval_type, actor_id, reason, correlation_id, approved_at)
            VALUES (
                '10400000-0000-0000-0000-000000000002',
                '10400000-0000-0000-0000-000000000001',
                'MOCK_TEST',
                'reviewer-w0104',
                'W-0104 owner-requested modern MOCK script',
                'W-0104-SCRIPT-V2',
                TIMESTAMPTZ '2026-08-22 02:50:00+00');
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            $"""
            DROP TRIGGER trg_ivr_script_approval_append_only ON ivr_script_approvals;
            DELETE FROM ivr_script_approvals WHERE id = '{ApprovalId}';
            CREATE TRIGGER trg_ivr_script_approval_append_only
            BEFORE UPDATE OR DELETE ON ivr_script_approvals
            FOR EACH ROW EXECUTE FUNCTION ivr_enforce_script_approval_append_only();

            DROP TRIGGER trg_ivr_script_version_lifecycle ON ivr_script_versions;
            DELETE FROM ivr_script_versions WHERE id = '{ScriptVersionId}';
            CREATE TRIGGER trg_ivr_script_version_lifecycle
            BEFORE UPDATE OR DELETE ON ivr_script_versions
            FOR EACH ROW EXECUTE FUNCTION ivr_enforce_script_version_lifecycle();
            """);
    }
}
