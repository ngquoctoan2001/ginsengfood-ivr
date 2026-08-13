# GitLab self-hosted Docker runner on Windows

This host runs GitLab Runner as a Windows service and uses Docker Desktop's Linux engine for IVR and Things CI jobs. A separate Ubuntu installation or WSL Ubuntu distribution is not required.

## Runner layout

| Runner | Project | Executor | Required job tag | Per-runner limit |
| --- | --- | --- | --- | --- |
| `ops-core-win` | `ginsengfood-ops-core` | Windows shell / PowerShell | `ops-core-win` | 1 |
| `ivr-docker-winhost` | `ginsengfood-ivr` | Docker Linux containers, privileged | `ginsengfood-docker` | 1 |
| `things-docker-winhost` | `things` | Docker Linux containers, privileged | `ginsengfood-docker` | 1 |

Global concurrency is 3. Each runner uses `request_concurrency=2`. Project runners are locked to their assigned project, do not run untagged jobs and remain unprotected until the corresponding protected-branch policy is configured.

## Host requirements

1. Windows and Docker Desktop must be running.
2. Docker Desktop must use Linux containers (`docker info --format '{{.OSType}}'` returns `linux`).
3. The Windows service `gitlab-runner` must be `Running` and `Automatic`.
4. `C:\GitLab-Runner\config.toml` is the active service configuration.
5. The host must have enough free CPU, RAM and disk for three single-job lanes. Reduce global concurrency if the machine becomes unstable.

If Docker Desktop is stopped, shell jobs for Ops Core may still run, but IVR/Things Docker jobs will fail or remain unavailable. Start Docker Desktop before expecting those pipelines to run.

## Safe verification

Run from PowerShell:

```powershell
docker info --format 'OSType={{.OSType}} Name={{.Name}}'
Get-Service gitlab-runner | Select-Object Name, Status, StartType
& 'C:\GitLab-Runner\gitlab-runner.exe' verify --config 'C:\GitLab-Runner\config.toml'
```

Use the GitLab project runner page to confirm `Online`, the expected version and the last-contact timestamp. Confirm a real job page names the project runner before claiming self-hosted execution.

Do **not** use `gitlab-runner list` as shareable evidence because some runner versions print authentication tokens. Never copy `config.toml`, runner tokens or environment secret files into the repository, screenshots or tickets.

## CI routing

IVR and Things define the tag at `default.tags`, so every job is routed to the project-locked Docker runner:

```yaml
default:
  tags:
    - ginsengfood-docker
```

Jobs with this tag do not fall back to GitLab SaaS instance runners. If the Windows host is offline, they remain pending instead of consuming SaaS compute minutes.

The Docker executor is privileged because current pipelines use Docker-in-Docker and Testcontainers. Only trusted project code may run on these runners. Do not enable untagged jobs or assign unrelated projects.

## Configuration changes and recovery

1. Back up `C:\GitLab-Runner\config.toml` before registration or manual edits.
2. GitLab Runner normally reloads `config.toml` automatically; confirm `Configuration loaded` in the Windows Application event log.
3. A service restart requires elevated PowerShell:

```powershell
Restart-Service gitlab-runner
```

4. After a failed configuration change, restore the last known-good backup, restart the service from elevated PowerShell and run `verify`.
5. If a runner authentication token might have been exposed, reset it in GitLab and update the local configuration immediately. Do not record the replacement value.

## Hosted evidence boundary

Runner `Online` proves control-plane contact only. Docker/DinD readiness requires a successful tagged pipeline containing at least:

- container image preparation;
- `.NET` build and tests;
- PostgreSQL Testcontainers or equivalent DinD use;
- security and privacy gates;
- GitLab job detail showing the expected self-hosted runner identity.

Protected branch, merge approval, `Pipelines must succeed`, Pages access control, registry and protected-variable evidence are independent platform gates.
