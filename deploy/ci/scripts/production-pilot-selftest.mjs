// Q-28 (PA2, 2026-09-26). Holds tools/ops/production-pilot-list.mjs - the operator's way to build the
// production pilot list and its approval hash - to the answers the C# implementation gives. The
// vectors are the ones UT-TRUNK-GATE-01 and IT-FLAG-PRODGATE-15 assert: the key is the bytes 1..32,
// 912345678 and 987654321 are the numbers. A drift between the two implementations would make an
// approved list silently match nothing, so it fails here instead.
import { fingerprint, isWellFormed, listHash, nationalDigits, readKey } from "../../../tools/ops/production-pilot-list.mjs";

const KEY = "AQIDBAUGBwgJCgsMDQ4PEBESExQVFhcYGRobHB0eHyA=";
const A = "pilot:jhapbmbcfcefgephpkljngbfbbhnobppnjlcdnnppcckmhnjclllmjdmhlmafnie";
const B = "pilot:mheofibkpehpjifmjfcgelphcgaonpboldpfoonoogfhodgnjpgiaiaenchinecb";
const BOTH = "BD23EB06EB346EE8EFA74A5B9507EC0A7F2CA4FE30C2D4E03E8CAB3DBD6B1905";

const failures = [];
const check = (condition, message) => {
  if (!condition) failures.push(message);
};

const key = readKey(KEY);
check(key !== null && key.length === 32, "the test key does not read as 32 bytes");
// Failures name a case by its position, never by the number: a failing run's output ends up in logs
// and evidence, and the evidence scan reads any of these spellings as a telephone number.
["0912345678", "+84912345678", "84912345678", "091 234 5678"].forEach((spelling, index) => {
  const digits = nationalDigits(spelling);
  check(digits === "912345678", `spelling ${index + 1} did not reduce to the national number`);
  check(digits !== null && fingerprint(key, digits) === A, `spelling ${index + 1} did not fingerprint as the C# code does`);
});
check(fingerprint(key, "987654321") === B, "the second number did not fingerprint as the C# code does");
check(isWellFormed(A) && isWellFormed(B) && !isWellFormed(A.toUpperCase()), "the fingerprint shape check disagrees");
check(!/[0-9]/.test(A) && !/[0-9]/.test(B), "a fingerprint contains a digit");
check(listHash([B, A]) === BOTH && listHash([A, B, A]) === BOTH, "the list hash depends on order or repeats");
check(listHash([A]) !== BOTH, "a list of one hashed like a list of two");
["", "12345", "+840912345678", "0912+345678", "091234567890", "0012345678"].forEach((refused, index) => {
  check(nationalDigits(refused) === null, `refusal case ${index + 1} was accepted as a number`);
});
check(readKey(Buffer.alloc(31).toString("base64")) === null, "a 31-byte key was accepted");
check(readKey("not base64!") === null, "a non-base64 key was accepted");

if (failures.length > 0) {
  for (const failure of failures) console.error(`PRODUCTION_PILOT_SELFTEST_FAIL: ${failure}`);
  process.exit(1);
}
console.log("PRODUCTION_PILOT_SELFTEST_PASS vectors=2 spellings=4 refusals=8 list_hash=ORDER_FREE");
