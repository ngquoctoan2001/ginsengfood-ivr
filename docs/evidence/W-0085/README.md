# W-0085 Linux ProjectReference path portability

Date: 2026-08-13
Origin: hosted GitLab pipeline failure
Status: `TESTS_PASS` locally and in a clean Linux SDK container

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

## Residual external gate

The source fix is locally complete at `TESTS_PASS`. A new hosted pipeline must
pass before this specific GitLab failure is closed. W-0061 remains
`BLOCKED_EXTERNAL` until the pipeline and remaining branch protection, approval,
registry, Pages access-control and protected-variable evidence are complete.
