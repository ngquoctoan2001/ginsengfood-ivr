using Ivr.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1062, CA1707, CA1861 // EF-generated migration shape.

namespace Ivr.Infrastructure.Persistence.Migrations;

/// <summary>
/// Adds the owner-accepted generic-customer greeting as a new immutable MOCK version.
/// Historical v1/v2 fixtures remain available for replay. This migration grants MOCK_TEST only.
/// </summary>
[DbContext(typeof(IvrDbContext))]
[Migration("20260822110000_W0104GenericCustomerGreetingScript")]
public sealed class W0104GenericCustomerGreetingScript : Migration
{
    private static readonly Guid ScriptVersionId =
        new("10400000-0000-0000-0000-000000000003");

    private static readonly Guid ApprovalId =
        new("10400000-0000-0000-0000-000000000004");

    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            INSERT INTO ivr_script_versions (
                id, template_id, version, status, template_text, template_hash,
                allowed_input_fields_json, created_by, create_reason, created_at,
                submitted_by, submit_reason, submitted_at)
            VALUES (
                '10400000-0000-0000-0000-000000000003',
                'SCRIPT-ORDER-CONFIRM',
                'v3-test-approved',
                'APPROVED',
                'Xin chào Quý khách. Đây là cuộc gọi tự động để xác nhận đơn hàng từ Ginsengfood. Quý khách có đơn hàng gồm {{items_spoken}}, tổng tiền {{total_amount_display}}, giao đến {{delivery_area_short}}. Bấm phím một để xác nhận đơn hàng, hoặc bấm phím không để hủy đơn hàng.',
                '2b98e5b695a03736e7749333e98099a71c7234c486494a37b7d2ca0dfe9dae51',
                '["customer_display_name","order_code_short","items[].public_name","items[].quantity","items[].unit_label","total_amount","currency","delivery_area_short","program_display_name","locale","pronunciation_hints"]',
                'owner-w0104',
                'W-0104 owner-accepted generic customer greeting',
                TIMESTAMPTZ '2026-08-22 04:10:00+00',
                'reviewer-w0104',
                'W-0104 generic greeting MOCK wording review',
                TIMESTAMPTZ '2026-08-22 04:10:00+00');

            INSERT INTO ivr_script_approvals (
                id, script_version_id, approval_type, actor_id, reason, correlation_id, approved_at)
            VALUES (
                '10400000-0000-0000-0000-000000000004',
                '10400000-0000-0000-0000-000000000003',
                'MOCK_TEST',
                'reviewer-w0104',
                'W-0104 owner-accepted generic customer greeting',
                'W-0104-SCRIPT-V3',
                TIMESTAMPTZ '2026-08-22 04:10:00+00');
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
