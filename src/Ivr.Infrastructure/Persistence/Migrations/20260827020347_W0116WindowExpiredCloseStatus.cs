using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ivr.Infrastructure.Persistence.Migrations;

/// <summary>
/// W-0116. Gives the confirmation-window sweep somewhere honest to close a job that ran out of
/// window without a channel shortage being the reason.
/// <para>
/// Before this, every job the sweep touched closed as CAPACITY_MISSED / CLOSED_CAPACITY, including
/// jobs held for admin review and dry runs -- neither of which ever wanted a channel. That spelled
/// a shortage into the one counter used to size the SIM order.
/// </para>
/// <para>
/// Up widens both closed vocabularies, so it cannot fail on existing rows. Down narrows them and
/// will refuse to run while any job still carries the new values, which is the correct outcome:
/// rolling the vocabulary back before the rows are re-closed would leave the table describing
/// states its own constraint forbids.
/// </para>
/// </summary>
public partial class W0116WindowExpiredCloseStatus : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        System.ArgumentNullException.ThrowIfNull(migrationBuilder);
        migrationBuilder.DropCheckConstraint(
            name: "ck_ivr_call_jobs_queue_status",
            table: "ivr_call_jobs");

        migrationBuilder.DropCheckConstraint(
            name: "ck_ivr_call_jobs_status",
            table: "ivr_call_jobs");

        migrationBuilder.AddCheckConstraint(
            name: "ck_ivr_call_jobs_queue_status",
            table: "ivr_call_jobs",
            sql: "queue_status IN ('QUEUED','HELD_MOCK','HELD_ELIGIBILITY','LEASED',"
                + "'HELD_LEASE_RECOVERY','HELD_NORMALIZATION','HELD_CALLBACK',"
                + "'HELD_TECHNICAL_REVIEW','HELD_CAPACITY','HELD_ADMIN_REVIEW',"
                + "'SKIPPED','BLOCKED','CLOSED_CAPACITY','CLOSED_WINDOW_EXPIRED')");

        migrationBuilder.AddCheckConstraint(
            name: "ck_ivr_call_jobs_status",
            table: "ivr_call_jobs",
            sql: "status IN ('CREATED','DRY_RUN','OPEN','QUEUED','READY_FOR_SCHEDULER',"
                + "'LEASED','LEASED_PENDING_DISPATCH','DISPATCH_LEASED','DIALING',"
                + "'ACTIVE_CALL','DISPOSITION_PENDING_NORMALIZATION',"
                + "'PROVIDER_EVENT_PENDING_NORMALIZATION','RESULT_READY_FOR_CALLBACK',"
                + "'TECHNICAL_RETRY_QUEUED','HELD_MOCK','HELD_ADMIN_REVIEW',"
                + "'HELD_ELIGIBILITY','HELD_CAPACITY','HELD_CALLBACK',"
                + "'HELD_TECHNICAL_REVIEW','HELD_NORMALIZATION','HELD_LEASE_RECOVERY',"
                + "'CAPACITY_HELD','CAPACITY_MISSED','CLOSED_CAPACITY','WINDOW_EXPIRED',"
                + "'RECOVERY_REQUIRED','BLOCKED','SKIPPED','CLOSED')");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        System.ArgumentNullException.ThrowIfNull(migrationBuilder);
        migrationBuilder.DropCheckConstraint(
            name: "ck_ivr_call_jobs_queue_status",
            table: "ivr_call_jobs");

        migrationBuilder.DropCheckConstraint(
            name: "ck_ivr_call_jobs_status",
            table: "ivr_call_jobs");

        migrationBuilder.AddCheckConstraint(
            name: "ck_ivr_call_jobs_queue_status",
            table: "ivr_call_jobs",
            sql: "queue_status IN ('QUEUED','HELD_MOCK','HELD_ELIGIBILITY','LEASED',"
                + "'HELD_LEASE_RECOVERY','HELD_NORMALIZATION','HELD_CALLBACK',"
                + "'HELD_TECHNICAL_REVIEW','HELD_CAPACITY','HELD_ADMIN_REVIEW',"
                + "'SKIPPED','BLOCKED','CLOSED_CAPACITY')");

        migrationBuilder.AddCheckConstraint(
            name: "ck_ivr_call_jobs_status",
            table: "ivr_call_jobs",
            sql: "status IN ('CREATED','DRY_RUN','OPEN','QUEUED','READY_FOR_SCHEDULER',"
                + "'LEASED','LEASED_PENDING_DISPATCH','DISPATCH_LEASED','DIALING',"
                + "'ACTIVE_CALL','DISPOSITION_PENDING_NORMALIZATION',"
                + "'PROVIDER_EVENT_PENDING_NORMALIZATION','RESULT_READY_FOR_CALLBACK',"
                + "'TECHNICAL_RETRY_QUEUED','HELD_MOCK','HELD_ADMIN_REVIEW',"
                + "'HELD_ELIGIBILITY','HELD_CAPACITY','HELD_CALLBACK',"
                + "'HELD_TECHNICAL_REVIEW','HELD_NORMALIZATION','HELD_LEASE_RECOVERY',"
                + "'CAPACITY_HELD','CAPACITY_MISSED','CLOSED_CAPACITY',"
                + "'RECOVERY_REQUIRED','BLOCKED','SKIPPED','CLOSED')");
    }
}
