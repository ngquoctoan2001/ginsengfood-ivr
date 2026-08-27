using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ivr.Infrastructure.Persistence.Migrations;

/// <summary>
/// W-0117. Moves the counted-attempt invariant from convention into the schema.
/// <para>
/// §16 says a technical, capacity, operational or policy result is never one of the customer's
/// attempts. <c>CallResultSnapshot.Create</c> enforces that for the normalizer, but the scheduler's
/// confirmation-window sweep builds the entity directly and never meets that guard, so the rule
/// held only for as long as every writer independently remembered it.
/// </para>
/// <para>
/// Unlike W-0116 this constraint NARROWS the accepted set, so it can reject rows that already
/// exist. The preflight runs first and names the offending result ids, because a bare check
/// violation on a table this size says only that something is wrong, not which rows or how many.
/// </para>
/// </summary>
public partial class W0117CountedAttemptInvariant : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        System.ArgumentNullException.ThrowIfNull(migrationBuilder);
        migrationBuilder.Sql(
            """
                DO $w0117$
                DECLARE
                    violations text;
                BEGIN
                    SELECT string_agg(
                               ivr_call_result_id || '=' || result_type,
                               '; ' ORDER BY ivr_call_result_id)
                    INTO violations
                    FROM ivr_call_results
                    WHERE is_counted_customer_attempt IS TRUE
                      AND result_type IN (
                          'IVR_TECHNICAL_EXCEPTION',
                          'IVR_CAPACITY_EXCEPTION',
                          'IVR_OPERATIONAL_BLOCKED',
                          'IVR_POLICY_BLOCKED');

                    IF violations IS NOT NULL THEN
                        RAISE EXCEPTION USING
                            ERRCODE = '23514',
                            MESSAGE = 'W-0117 counted-attempt preflight blocked: ' || violations;
                    END IF;
                END
                $w0117$;
                """);

        migrationBuilder.AddCheckConstraint(
            name: "ck_ivr_call_results_non_customer_not_counted",
            table: "ivr_call_results",
            sql: "result_type NOT IN ('IVR_TECHNICAL_EXCEPTION','IVR_CAPACITY_EXCEPTION',"
                + "'IVR_OPERATIONAL_BLOCKED','IVR_POLICY_BLOCKED')"
                + " OR is_counted_customer_attempt IS FALSE");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        System.ArgumentNullException.ThrowIfNull(migrationBuilder);
        migrationBuilder.DropCheckConstraint(
            name: "ck_ivr_call_results_non_customer_not_counted",
            table: "ivr_call_results");
    }
}
