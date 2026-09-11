# API Changelog 1.0.0-draft.25 vs. 1.0.0-draft.26

> **PLACEHOLDER — W-0277. This file is knowingly wrong and `api_contract_diff` will fail on it.**
> The real body is produced by `oasdiff`, which exists only inside the job's pinned image and
> cannot be installed on a developer host — `deploy/ci/gate-invocations.json` records the same
> constraint for `selftest-oasdiff.sh`. The job now `cat`s the generated file before comparing it,
> so the next red run prints the body; commit that output here verbatim.
>
> It is **not** hand-written on purpose: the job compares byte-for-byte, so a plausible-looking
> guess would be both wrong and undetectable until it was.
