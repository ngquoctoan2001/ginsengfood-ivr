# W-0085 Linux ProjectReference path portability

Date: 2026-08-13
Origin: hosted GitLab pipeline failure
Status: `ACCEPTED` — local, clean Linux-container and hosted GitLab regression evidence passed

## Failure evidence

GitLab job `15870797229` ran commit `2b1a4d4` with GitLab Runner
`19.2.0-pre` on `green-8.saas-linux-small-amd64` using the Docker executor.
`UT-BOOT-03` failed while reading MSBuild `ProjectReference` values:

```text
Expected: Ivr.Contracts, Ivr.Infrastructure
Actual:   ..\Ivr.Contracts\Ivr.Contracts,
          ..\Ivr.Infrastructure\Ivr.Infrastructure
```

The `.csproj` files correctly use Windows-style relative includes. The test
passed on Windows because `Path.GetFullPath` treats backslash as a directory
separator there. On Linux, backslash is an ordinary filename character, so the
test extracted the entire unresolved relative string as the project name.

This was a test portability defect, not an invalid source-project dependency.

## Fix

`ArchitectureDependencyTests` now normalizes both `\` and `/` to the current
platform directory separator before resolving the referenced project name.
`UT-BOOT-03-LINUX-PATH` covers both include forms explicitly.

## Verification

```text
Windows focused ArchitectureDependencyTests: 3/3 PASS
Linux mcr.microsoft.com/dotnet/sdk:10.0.201 focused: 3/3 PASS
Linux full unit suite: 56/56 PASS
Locked restore: PASS
Release build: 0 warnings, 0 errors
Full local solution: contract 19/19, unit 56/56, integration 23/23
Full local total: 98/98 PASS
dotnet format --verify-no-changes: PASS
CI configuration self-test: PASS
```

The Linux verification copied the working tree without host `bin/obj` output
into a disposable container before restore/build/test. This avoids a false pass
from Windows-generated build assets.

## Hosted closure

The source portability defect is closed by two successful hosted pipelines:

```text
Pipeline #2756119982: PASS on GitLab SaaS Linux executor
Pipeline #2756183002: PASS on self-hosted Linux-container runner #55115499
Jobs: 9/9 PASS
Tests: 98/98 PASS
Self-hosted merged coverage: 91.5%
```

Pipeline `#2756183002` also passed the PostgreSQL Testcontainers build job through the privileged Docker executor, so the corrected architecture test is proven in the final self-hosted CI topology rather than only in a disposable local container.

Codex accepted W-0085 on 2026-08-13 under the IVR owner's standing authorization to self-review and close completed prompt/remediation work.

## Residual external gate

W-0085 is `ACCEPTED`. W-0061 remains independently `BLOCKED_EXTERNAL` until branch protection, approval, merge enforcement, registry, Pages access-control and protected-variable evidence are complete. Those platform settings are not residual defects in the W-0085 source fix.
