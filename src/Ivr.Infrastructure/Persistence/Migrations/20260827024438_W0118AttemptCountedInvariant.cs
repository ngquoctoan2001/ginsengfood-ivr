using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ivr.Infrastructure.Persistence.Migrations;

/// <summary>
/// W-0118. Extends the W-0117 counted-attempt invariant to <c>ivr_call_attempts</c>.
/// <para>
/// This table is not a reporting mirror of <c>ivr_call_results</c>. The scheduler counts rows here
/// — <c>attempt.is_counted_customer_attempt IS TRUE</c> in TryClaimDueDispatchAsync — to decide
/// whether the customer is still owed a call. A non-customer outcome counted on this table spends
/// one of the two attempts the policy promised, so the order can reach its final attempt without
/// the customer's phone having rung twice.
/// </para>
/// <para>
/// The existing <c>ck_ivr_call_attempts_technical_not_counted</c> is kept rather than replaced.
/// It keys off <c>technical_exception_type</c> and so still covers a technically failed attempt
/// whose <c>result_status</c> has not been written yet; this one covers the capacity, operational
/// and policy outcomes, which carry no exception type and were left uncovered.
/// </para>
/// <para>
/// Narrowing, like W-0117, so the preflight runs first and names the offending attempt ids.
/// </para>
/// </summary>
public partial class W0118AttemptCountedInvariant : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        System.ArgumentNullException.ThrowIfNull(migrationBuilder);
        migrationBuilder.Sql(
            """
                DO $w0118$
                DECLARE
                    violations text;
                BEGIN
                    SELECT string_agg(
                               ivr_call_attempt_id || '=' || result_status,
                               '; ' ORDER BY ivr_call_attempt_id)
                    INTO violations
                    FROM ivr_call_attempts
                    WHERE is_counted_customer_attempt IS TRUE
                      AND result_status IN (
                          'IVR_TECHNICAL_EXCEPTION',
                          'IVR_CAPACITY_EXCEPTION',
                          'IVR_OPERATIONAL_BLOCKED',
                          'IVR_POLICY_BLOCKED');

                    IF violations IS NOT NULL THEN
                        RAISE EXCEPTION USING
                            ERRCODE = '23514',
                            MESSAGE = 'W-0118 counted-attempt preflight blocked: ' || violations;
                    END IF;
                END
                $w0118$;
                """);

        migrationBuilder.AddCheckConstraint(
            name: "ck_ivr_call_attempts_non_customer_not_counted",
            table: "ivr_call_attempts",
            sql: "result_status IS NULL"
                + " OR result_status NOT IN ('IVR_TECHNICAL_EXCEPTION','IVR_CAPACITY_EXCEPTION',"
                + "'IVR_OPERATIONAL_BLOCKED','IVR_POLICY_BLOCKED')"
                + " OR is_counted_customer_attempt IS FALSE");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        System.ArgumentNullException.ThrowIfNull(migrationBuilder);
        migrationBuilder.DropCheckConstraint(
            name: "ck_ivr_call_attempts_non_customer_not_counted",
            table: "ivr_call_attempts");
    }
}
