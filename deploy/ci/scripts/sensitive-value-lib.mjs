/**
 * The one definition of "this string looks like personal data or a credential".
 *
 * There were five, in five shapes, and the shapes were not the problem — the coverage was.
 * `assertIdentifier` existed in seven copies and only two of them ran this check at all, so the
 * same field name was screened for secrets in two validators and waved through in five. Same
 * function name, same documented purpose, opposite security posture, and nothing in the repository
 * said which one was the rule.
 *
 * The owner settled it on `2026-09-09`: the copies that check are the rule. That decision is only
 * meaningful if there is one thing to point at, which is this file.
 *
 * The union of the five is taken, with one exclusion. `target-v1-shared-e2e-report-validator` also
 * refused `[?#]` from the same function, under the message "must not contain a URL query or
 * fragment" — that is URL hygiene, not a secret, and folding it in here would have rejected every
 * identifier containing a question mark in six validators that never asked for it. It stays where
 * it was, under its own name.
 */

/**
 * Patterns are ordered so the most specific reason wins: a JWT is credential-like and also
 * matches nothing else, but an address containing digits would otherwise be reported as a phone.
 */
const PATTERNS = [
  [/[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,}/iu, "an email-like value"],

  // `bearer` is accepted followed by whitespace OR by a separator: the narrower `bearer\s+` form
  // that four of the five copies used misses `BEARER:token`, and one copy had already been widened
  // to catch it. Taking the wider one is the whole point of unifying them.
  [
    /(?:password|passwd|bearer(?:\s+|[:=])|api[_ -]?key|access[_ -]?token|private[_ -]?key|client[_ -]?secret)\s*[:=]?/iu,
    "credential- or secret-like material",
  ],

  // A JWT is recognisable on sight and three of the five copies looked for one.
  [
    /\beyJ[A-Za-z0-9_-]{10,}\.[A-Za-z0-9_-]{10,}\.[A-Za-z0-9_-]{10,}\b/u,
    "credential- or secret-like material",
  ],

  [
    /\b\d{1,5}\s+(?:đường|duong|phố|pho|street|st\.?|road|rd\.?|avenue|ave\.?)\b/iu,
    "a street-address-like value",
  ],

  [/(?:^|\D)(?:\+?\d[\s().-]*){9,15}(?:$|\D)/u, "a phone-like value"],
];

/**
 * An ISO calendar date, optionally with a time.
 *
 * These are cut out of a value before the phone test runs, and the reason is a real regression
 * rather than a precaution. The phone pattern counts nine to fifteen digits separated by anything
 * in `[\s().-]`, and a dash is in that set — so `CONFIG-2026-09-04-01`, a perfectly ordinary
 * config version, reads as a ten-digit phone number. Applying the check to a validator that had
 * never run it turned that identifier into a refusal.
 *
 * The year, month and day ranges are pinned rather than left as `\d{4}-\d{2}-\d{2}`, because the
 * loose form is an evasion: `0912-34-5678` would match it, and stripping that would hide eight
 * digits of a real number. `19xx`/`20xx` with a month of `01`–`12` and a day of `01`–`31` cannot
 * be arranged out of a phone number.
 */
const ISO_DATE = /(?:19|20)\d{2}-(?:0[1-9]|1[0-2])-(?:0[1-9]|[12]\d|3[01])(?:T[\d:.]+(?:Z|[+-]\d{2}:?\d{2})?)?/gu;

/**
 * Describes what was found, or returns null.
 *
 * Deliberately not a `fail()` of its own: every validator has one, with its own exit code and
 * message prefix, and a shared helper that terminated the process would have to pick one of them.
 * Returning the reason lets each caller keep its own convention while sharing the rule.
 *
 * @param {string} value
 * @returns {string | null} a phrase completing "<label> contains ...", or null when clean.
 */
export function findSensitiveValue(value) {
  if (typeof value !== "string") {
    return null;
  }

  // Dates are removed for the phone test only. Every other pattern sees the value untouched, so
  // nothing can be smuggled past them by embedding a date next to it.
  const withoutDates = value.replace(ISO_DATE, " ");
  for (const [pattern, reason] of PATTERNS) {
    const subject = reason === "a phone-like value" ? withoutDates : value;
    if (pattern.test(subject)) {
      return reason;
    }
  }

  return null;
}
