using System.Security.Cryptography;
using System.Text;
using Ivr.Domain.Confirmation;

namespace Ivr.Infrastructure.Contracts;

/// <summary>
/// W-0312. What a task that reached IVR with a number stores where a protected dial token would
/// otherwise go: <c>enc:direct:</c> followed by the SHA-256 of its <c>task_id</c>.
/// <para>
/// One per task is the whole design. The resolve ledger binds a token hash to the first task that
/// dials with it and refuses every other task presenting the same hash
/// (<c>PostgresDialTokenResolveLedger</c>), so a single shared placeholder would let exactly one
/// number-only task dial and refuse the rest. Derived from <c>task_id</c>, the reference is stable
/// across a worker restart, distinct for every task, and carries no customer data - the number
/// itself lives in <c>phone_e164</c> and is read on the dial path only.
/// </para>
/// <para>
/// Hex for the same reason the protectors emit hex: the raw-phone guard inside
/// <see cref="DialTokenReference"/> looks for a digit run bounded by non-alphanumerics, and a run
/// inside a hex string never is.
/// </para>
/// </summary>
internal static class DirectDialReference
{
    private const string Prefix = "enc:direct:";

    public static string ValueFor(string taskId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(taskId);
        return string.Concat(
            Prefix,
            Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(taskId))));
    }

    /// <summary>
    /// The snapshot form. It expires with the confirmation window, which is exactly what intake
    /// requires of a real token since OD-V1-17.
    /// </summary>
    public static DialTokenReference Create(string taskId, DateTimeOffset confirmationWindowExpiresAt) =>
        DialTokenReference.Create(ValueFor(taskId), confirmationWindowExpiresAt);
}
