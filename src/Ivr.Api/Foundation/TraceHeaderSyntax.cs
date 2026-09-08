using Ivr.Domain.Privacy;

namespace Ivr.Api.Foundation;

/// <summary>
/// The one rule for <c>Idempotency-Key</c> and <c>X-Correlation-Id</c>: 1..128 characters drawn
/// from <c>[A-Za-z0-9._:-]</c>, and safe to write into an audit row.
/// <para>
/// W-0221. This lived in three places - the task intake endpoint, the internal request guard and
/// the correlation middleware - and two of them agreed. The internal guard checked length and
/// PII-safety but not the alphabet, so the same header was accepted at the admin door and refused
/// at the intake door. A base64 key, which is an ordinary way to mint one, worked on one and not
/// the other; nothing in the schema could describe both, because the two routes share a single
/// OpenAPI parameter.
/// </para>
/// <para>
/// The owner settled it on 2026-09-07 by tightening admin to match intake, and the durable half of
/// that decision is this file rather than a third copy of the predicate: three copies are how they
/// drifted apart in the first place, and a rule that is written down once cannot disagree with
/// itself.
/// </para>
/// <para>
/// Why this alphabet: these values are written into audit rows, log scopes and idempotency keys,
/// and they are echoed back to callers. Restricting them to characters that need no escaping in
/// any of those places is what makes echoing them safe. It is deliberately narrower than HTTP
/// allows.
/// </para>
/// </summary>
public static class TraceHeaderSyntax
{
    /// <summary>Longest accepted value. Also the <c>maxLength</c> the OpenAPI parameters carry.</summary>
    public const int MaxLength = 128;

    /// <summary>
    /// True when <paramref name="value"/> may be accepted as a trace or idempotency header.
    /// Empty is false; callers report a missing header separately from a malformed one, because
    /// "you forgot it" and "yours is the wrong shape" are different problems for whoever is
    /// holding the failing request.
    /// </summary>
    public static bool IsValid(string? value) =>
        !string.IsNullOrEmpty(value)
        && value.Length <= MaxLength
        && PiiGuard.IsSafeText(value)
        && value.All(character =>
            char.IsAsciiLetterOrDigit(character)
            || character is '-' or '_' or '.' or ':');
}
