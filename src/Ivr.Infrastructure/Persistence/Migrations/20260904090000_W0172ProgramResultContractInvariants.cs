using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ivr.Infrastructure.Persistence.Migrations;

/// <summary>
/// W-0172. Makes the signed 11/9/6 result taxonomy a database invariant.
/// </summary>
[DbContext(typeof(IvrDbContext))]
[Migration("20260904090000_W0172ProgramResultContractInvariants")]
public partial class W0172ProgramResultContractInvariants : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        ArgumentNullException.ThrowIfNull(migrationBuilder);
        migrationBuilder.Sql(
            """
                DO $w0172$
                DECLARE
                    violations text;
                BEGIN
                    SELECT string_agg(item, '; ' ORDER BY item)
                    INTO violations
                    FROM (
                        SELECT 'attempt:' || ivr_call_attempt_id || '=' || result_status
                            || '/counted=' || is_counted_customer_attempt::text AS item
                        FROM ivr_call_attempts
                        WHERE result_status IS NOT NULL
                          AND (
                              result_status NOT IN (
                                  'IVR_CONFIRMED',
                                  'IVR_CUSTOMER_CANCELLED',
                                  'IVR_NO_ANSWER_ATTEMPT',
                                  'IVR_NO_ANSWER_FINAL',
                                  'IVR_CONFIRMATION_WINDOW_EXPIRED',
                                  'IVR_INVALID_PHONE_FINAL',
                                  'IVR_WRONG_INPUT',
                                  'IVR_TECHNICAL_EXCEPTION',
                                  'IVR_CAPACITY_EXCEPTION')
                              OR is_counted_customer_attempt IS DISTINCT FROM
                                  (result_status IN (
                                      'IVR_CONFIRMED',
                                      'IVR_CUSTOMER_CANCELLED',
                                      'IVR_NO_ANSWER_ATTEMPT',
                                      'IVR_NO_ANSWER_FINAL',
                                      'IVR_WRONG_INPUT')))
                        UNION ALL
                        SELECT 'result:' || ivr_call_result_id || '=' || result_type
                            || '/counted=' || is_counted_customer_attempt::text
                            || '/final=' || is_final_for_ivr::text AS item
                        FROM ivr_call_results
                        WHERE result_type NOT IN (
                                  'IVR_CONFIRMED',
                                  'IVR_CUSTOMER_CANCELLED',
                                  'IVR_NO_ANSWER_ATTEMPT',
                                  'IVR_NO_ANSWER_FINAL',
                                  'IVR_CONFIRMATION_WINDOW_EXPIRED',
                                  'IVR_INVALID_PHONE_FINAL',
                                  'IVR_WRONG_INPUT',
                                  'IVR_TECHNICAL_EXCEPTION',
                                  'IVR_CAPACITY_EXCEPTION')
                           OR is_counted_customer_attempt IS DISTINCT FROM
                              (result_type IN (
                                  'IVR_CONFIRMED',
                                  'IVR_CUSTOMER_CANCELLED',
                                  'IVR_NO_ANSWER_ATTEMPT',
                                  'IVR_NO_ANSWER_FINAL',
                                  'IVR_WRONG_INPUT'))
                           OR is_final_for_ivr IS DISTINCT FROM
                              (result_type IN (
                                  'IVR_CONFIRMED',
                                  'IVR_CUSTOMER_CANCELLED',
                                  'IVR_NO_ANSWER_FINAL',
                                  'IVR_CONFIRMATION_WINDOW_EXPIRED',
                                  'IVR_INVALID_PHONE_FINAL',
                                  'IVR_CAPACITY_EXCEPTION'))
                           OR CASE result_type
                                  WHEN 'IVR_CONFIRMED' THEN
                                      recommended_core_action = 'REVALIDATE_AND_CONFIRM_ORDER'
                                  WHEN 'IVR_CUSTOMER_CANCELLED' THEN
                                      recommended_core_action =
                                          'REVALIDATE_AND_CANCEL_CUSTOMER_REQUEST'
                                  WHEN 'IVR_NO_ANSWER_ATTEMPT' THEN
                                      recommended_core_action = 'NO_STATE_CHANGE_WAIT_FOR_TIMEOUT'
                                  WHEN 'IVR_NO_ANSWER_FINAL' THEN
                                      recommended_core_action = 'NO_STATE_CHANGE_WAIT_FOR_TIMEOUT'
                                  WHEN 'IVR_WRONG_INPUT' THEN
                                      recommended_core_action = 'NO_STATE_CHANGE_WAIT_FOR_TIMEOUT'
                                  WHEN 'IVR_CONFIRMATION_WINDOW_EXPIRED' THEN
                                      recommended_core_action IN (
                                          'REVALIDATE_AND_EXPIRE_CONFIRMATION',
                                          'REVALIDATE_AND_HOLD_ADMIN_REVIEW')
                                  WHEN 'IVR_INVALID_PHONE_FINAL' THEN
                                      recommended_core_action =
                                          'REVALIDATE_AND_HOLD_ADMIN_REVIEW'
                                  WHEN 'IVR_TECHNICAL_EXCEPTION' THEN
                                      recommended_core_action =
                                          'REVALIDATE_AND_HOLD_ADMIN_REVIEW'
                                  WHEN 'IVR_CAPACITY_EXCEPTION' THEN
                                      recommended_core_action =
                                          'REVALIDATE_AND_HOLD_ADMIN_REVIEW'
                                  ELSE false
                              END IS NOT TRUE
                        UNION ALL
                        SELECT 'callback:' || callback_id || '=' || result_status AS item
                        FROM ivr_result_callbacks
                        WHERE result_status NOT IN (
                            'IVR_CONFIRMED',
                            'IVR_CUSTOMER_CANCELLED',
                            'IVR_NO_ANSWER_FINAL',
                            'IVR_CONFIRMATION_WINDOW_EXPIRED',
                            'IVR_INVALID_PHONE_FINAL',
                            'IVR_CAPACITY_EXCEPTION')
                    ) AS invalid;

                    IF violations IS NOT NULL THEN
                        RAISE EXCEPTION USING
                            ERRCODE = '23514',
                            MESSAGE = 'W-0172 program/result preflight blocked: ' || violations;
                    END IF;
                END
                $w0172$;
                """);

        migrationBuilder.DropCheckConstraint(
            name: "ck_ivr_call_attempts_non_customer_not_counted",
            table: "ivr_call_attempts");
        migrationBuilder.DropCheckConstraint(
            name: "ck_ivr_call_attempts_result_status",
            table: "ivr_call_attempts");
        migrationBuilder.DropCheckConstraint(
            name: "ck_ivr_call_results_non_customer_not_counted",
            table: "ivr_call_results");
        migrationBuilder.DropCheckConstraint(
            name: "ck_ivr_call_results_result_type",
            table: "ivr_call_results");
        migrationBuilder.DropCheckConstraint(
            name: "ck_ivr_result_callbacks_result_status",
            table: "ivr_result_callbacks");

        migrationBuilder.AddCheckConstraint(
            name: "ck_ivr_call_attempts_counted_matches_type",
            table: "ivr_call_attempts",
            sql: "result_status IS NULL"
                + " OR (is_counted_customer_attempt IS TRUE AND result_status IN ("
                + "'IVR_CONFIRMED','IVR_CUSTOMER_CANCELLED','IVR_NO_ANSWER_ATTEMPT',"
                + "'IVR_NO_ANSWER_FINAL','IVR_WRONG_INPUT'))"
                + " OR (is_counted_customer_attempt IS FALSE AND result_status IN ("
                + "'IVR_CONFIRMATION_WINDOW_EXPIRED','IVR_INVALID_PHONE_FINAL',"
                + "'IVR_TECHNICAL_EXCEPTION','IVR_CAPACITY_EXCEPTION'))");
        migrationBuilder.AddCheckConstraint(
            name: "ck_ivr_call_attempts_result_status",
            table: "ivr_call_attempts",
            sql: "result_status IS NULL OR result_status IN ('IVR_CONFIRMED',"
                + "'IVR_CUSTOMER_CANCELLED','IVR_NO_ANSWER_ATTEMPT','IVR_NO_ANSWER_FINAL',"
                + "'IVR_CONFIRMATION_WINDOW_EXPIRED','IVR_INVALID_PHONE_FINAL',"
                + "'IVR_WRONG_INPUT','IVR_TECHNICAL_EXCEPTION','IVR_CAPACITY_EXCEPTION')");
        migrationBuilder.AddCheckConstraint(
            name: "ck_ivr_call_results_action_matches_type",
            table: "ivr_call_results",
            sql: "(result_type = 'IVR_CONFIRMED' AND recommended_core_action = "
                + "'REVALIDATE_AND_CONFIRM_ORDER')"
                + " OR (result_type = 'IVR_CUSTOMER_CANCELLED' AND recommended_core_action = "
                + "'REVALIDATE_AND_CANCEL_CUSTOMER_REQUEST')"
                + " OR (result_type IN ('IVR_NO_ANSWER_ATTEMPT','IVR_NO_ANSWER_FINAL',"
                + "'IVR_WRONG_INPUT') AND recommended_core_action = "
                + "'NO_STATE_CHANGE_WAIT_FOR_TIMEOUT')"
                + " OR (result_type = 'IVR_CONFIRMATION_WINDOW_EXPIRED' AND "
                + "recommended_core_action IN ('REVALIDATE_AND_EXPIRE_CONFIRMATION',"
                + "'REVALIDATE_AND_HOLD_ADMIN_REVIEW'))"
                + " OR (result_type IN ('IVR_INVALID_PHONE_FINAL','IVR_TECHNICAL_EXCEPTION',"
                + "'IVR_CAPACITY_EXCEPTION') AND recommended_core_action = "
                + "'REVALIDATE_AND_HOLD_ADMIN_REVIEW')");
        migrationBuilder.AddCheckConstraint(
            name: "ck_ivr_call_results_counted_matches_type",
            table: "ivr_call_results",
            sql: "(is_counted_customer_attempt IS TRUE AND result_type IN ("
                + "'IVR_CONFIRMED','IVR_CUSTOMER_CANCELLED','IVR_NO_ANSWER_ATTEMPT',"
                + "'IVR_NO_ANSWER_FINAL','IVR_WRONG_INPUT'))"
                + " OR (is_counted_customer_attempt IS FALSE AND result_type IN ("
                + "'IVR_CONFIRMATION_WINDOW_EXPIRED','IVR_INVALID_PHONE_FINAL',"
                + "'IVR_TECHNICAL_EXCEPTION','IVR_CAPACITY_EXCEPTION'))");
        migrationBuilder.AddCheckConstraint(
            name: "ck_ivr_call_results_finality_matches_type",
            table: "ivr_call_results",
            sql: "(is_final_for_ivr IS TRUE AND result_type IN ("
                + "'IVR_CONFIRMED','IVR_CUSTOMER_CANCELLED','IVR_NO_ANSWER_FINAL',"
                + "'IVR_CONFIRMATION_WINDOW_EXPIRED','IVR_INVALID_PHONE_FINAL',"
                + "'IVR_CAPACITY_EXCEPTION'))"
                + " OR (is_final_for_ivr IS FALSE AND result_type IN ("
                + "'IVR_NO_ANSWER_ATTEMPT','IVR_WRONG_INPUT','IVR_TECHNICAL_EXCEPTION'))");
        migrationBuilder.AddCheckConstraint(
            name: "ck_ivr_call_results_result_type",
            table: "ivr_call_results",
            sql: "result_type IN ('IVR_CONFIRMED','IVR_CUSTOMER_CANCELLED',"
                + "'IVR_NO_ANSWER_ATTEMPT','IVR_NO_ANSWER_FINAL',"
                + "'IVR_CONFIRMATION_WINDOW_EXPIRED','IVR_INVALID_PHONE_FINAL',"
                + "'IVR_WRONG_INPUT','IVR_TECHNICAL_EXCEPTION','IVR_CAPACITY_EXCEPTION')");
        migrationBuilder.AddCheckConstraint(
            name: "ck_ivr_result_callbacks_result_status",
            table: "ivr_result_callbacks",
            sql: "result_status IN ('IVR_CONFIRMED','IVR_CUSTOMER_CANCELLED',"
                + "'IVR_NO_ANSWER_FINAL','IVR_CONFIRMATION_WINDOW_EXPIRED',"
                + "'IVR_INVALID_PHONE_FINAL','IVR_CAPACITY_EXCEPTION')");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        ArgumentNullException.ThrowIfNull(migrationBuilder);
        migrationBuilder.DropCheckConstraint(
            name: "ck_ivr_call_attempts_counted_matches_type",
            table: "ivr_call_attempts");
        migrationBuilder.DropCheckConstraint(
            name: "ck_ivr_call_attempts_result_status",
            table: "ivr_call_attempts");
        migrationBuilder.DropCheckConstraint(
            name: "ck_ivr_call_results_action_matches_type",
            table: "ivr_call_results");
        migrationBuilder.DropCheckConstraint(
            name: "ck_ivr_call_results_counted_matches_type",
            table: "ivr_call_results");
        migrationBuilder.DropCheckConstraint(
            name: "ck_ivr_call_results_finality_matches_type",
            table: "ivr_call_results");
        migrationBuilder.DropCheckConstraint(
            name: "ck_ivr_call_results_result_type",
            table: "ivr_call_results");
        migrationBuilder.DropCheckConstraint(
            name: "ck_ivr_result_callbacks_result_status",
            table: "ivr_result_callbacks");

        migrationBuilder.AddCheckConstraint(
            name: "ck_ivr_call_attempts_non_customer_not_counted",
            table: "ivr_call_attempts",
            sql: "result_status IS NULL"
                + " OR result_status NOT IN ('IVR_TECHNICAL_EXCEPTION','IVR_CAPACITY_EXCEPTION',"
                + "'IVR_OPERATIONAL_BLOCKED','IVR_POLICY_BLOCKED')"
                + " OR is_counted_customer_attempt IS FALSE");
        migrationBuilder.AddCheckConstraint(
            name: "ck_ivr_call_attempts_result_status",
            table: "ivr_call_attempts",
            sql: "result_status IS NULL OR result_status IN ('IVR_CONFIRMED',"
                + "'IVR_CUSTOMER_CANCELLED','IVR_NO_ANSWER_ATTEMPT','IVR_NO_ANSWER_FINAL',"
                + "'IVR_CONFIRMATION_WINDOW_EXPIRED','IVR_INVALID_PHONE_FINAL',"
                + "'IVR_WRONG_INPUT','IVR_TECHNICAL_EXCEPTION','IVR_CAPACITY_EXCEPTION',"
                + "'IVR_OPERATIONAL_BLOCKED','IVR_POLICY_BLOCKED')");
        migrationBuilder.AddCheckConstraint(
            name: "ck_ivr_call_results_non_customer_not_counted",
            table: "ivr_call_results",
            sql: "result_type NOT IN ('IVR_TECHNICAL_EXCEPTION','IVR_CAPACITY_EXCEPTION',"
                + "'IVR_OPERATIONAL_BLOCKED','IVR_POLICY_BLOCKED')"
                + " OR is_counted_customer_attempt IS FALSE");
        migrationBuilder.AddCheckConstraint(
            name: "ck_ivr_call_results_result_type",
            table: "ivr_call_results",
            sql: "result_type IN ('IVR_CONFIRMED','IVR_CUSTOMER_CANCELLED',"
                + "'IVR_NO_ANSWER_ATTEMPT','IVR_NO_ANSWER_FINAL',"
                + "'IVR_CONFIRMATION_WINDOW_EXPIRED','IVR_INVALID_PHONE_FINAL',"
                + "'IVR_WRONG_INPUT','IVR_TECHNICAL_EXCEPTION','IVR_CAPACITY_EXCEPTION',"
                + "'IVR_OPERATIONAL_BLOCKED','IVR_POLICY_BLOCKED')");
        migrationBuilder.AddCheckConstraint(
            name: "ck_ivr_result_callbacks_result_status",
            table: "ivr_result_callbacks",
            sql: "result_status IN ('IVR_CONFIRMED','IVR_CUSTOMER_CANCELLED',"
                + "'IVR_NO_ANSWER_ATTEMPT','IVR_NO_ANSWER_FINAL',"
                + "'IVR_CONFIRMATION_WINDOW_EXPIRED','IVR_INVALID_PHONE_FINAL',"
                + "'IVR_WRONG_INPUT','IVR_TECHNICAL_EXCEPTION','IVR_CAPACITY_EXCEPTION',"
                + "'IVR_OPERATIONAL_BLOCKED','IVR_POLICY_BLOCKED')");
    }
}
