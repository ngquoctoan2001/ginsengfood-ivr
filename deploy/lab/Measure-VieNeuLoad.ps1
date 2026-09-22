[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$Image,
    [Parameter(Mandatory)][string]$ModelBundle,
    [Parameter(Mandatory)][string]$VoiceManifest,
    [Parameter(Mandatory)][string]$WorkerTexts,
    [Parameter(Mandatory)][string]$OutputDirectory,
    [ValidateSet('LOCAL_LAB','S5_TARGET')][string]$EvidenceScope='LOCAL_LAB',
    [switch]$OwnerConfirmedS5,
    [ValidateRange(0,64)][int]$CpuLimit=0,
    [ValidateRange(0,128)][int]$MemoryGiB=0
)
$ErrorActionPreference='Stop'
if($EvidenceScope -eq 'S5_TARGET' -and -not $OwnerConfirmedS5){throw 'S5 needs an explicit owner-confirmed target; do not relabel local measurements.'}
if($Image -notmatch '^sha256:[a-f0-9]{64}$'){throw 'Use the exact local image ID, not a mutable tag.'}
if(Test-Path -LiteralPath $OutputDirectory){throw 'Use a new evidence directory.'}
$modelPath=(Resolve-Path -LiteralPath $ModelBundle).Path
$manifestPath=(Resolve-Path -LiteralPath $VoiceManifest).Path
$textsPath=(Resolve-Path -LiteralPath $WorkerTexts).Path
$probePath=(Resolve-Path -LiteralPath (Join-Path $PSScriptRoot '../tts/tests/probe_load.py')).Path
$manifest=Get-Content -LiteralPath $manifestPath -Raw -Encoding utf8|ConvertFrom-Json
$voiceIds=(@('North','Central','South')|ForEach-Object {$manifest.selections.$_.voice_id}) -join ','
$actual=(& docker image inspect $Image --format '{{.Id}}').Trim()
if($LASTEXITCODE -ne 0 -or $actual -ne $Image){throw 'Exact image missing.'}
New-Item -ItemType Directory -Path $OutputDirectory | Out-Null
$outPath=(Resolve-Path -LiteralPath $OutputDirectory).Path
$containerName='ivr-vieneu-load-'+[Guid]::NewGuid().ToString('N').Substring(0,8)
$cpu=Get-CimInstance Win32_Processor|Select-Object Name,NumberOfCores,NumberOfLogicalProcessors
$hostInfo=[ordered]@{scope=$EvidenceScope;owner_confirmed_s5=[bool]$OwnerConfirmedS5;host_name=$env:COMPUTERNAME;cpu=$cpu;host_memory_bytes=(Get-CimInstance Win32_ComputerSystem).TotalPhysicalMemory;docker=(& docker info --format '{{json .NCPU}} {{json .MemTotal}}');image=$actual;cpu_limit=$CpuLimit;memory_gib=$MemoryGiB;worker_texts_sha256=(Get-FileHash -LiteralPath $textsPath -Algorithm SHA256).Hash.ToLowerInvariant();probe_sha256=(Get-FileHash -LiteralPath $probePath -Algorithm SHA256).Hash.ToLowerInvariant();started_utc=[DateTimeOffset]::UtcNow.ToString('o');real_customer_call_allowed='NO'}
$hostInfo|ConvertTo-Json -Depth 10|Set-Content -LiteralPath (Join-Path $outPath 'host.json') -Encoding utf8
$limits=@()
if($CpuLimit -gt 0){$limits+=@('--cpus',"$CpuLimit")}
if($MemoryGiB -gt 0){$limits+=@('--memory',"${MemoryGiB}g",'--memory-swap',"${MemoryGiB}g")}
# This test has no networking, SIP destination, Docker socket, model writes or audio output.
& docker run --rm --name $containerName --network none --read-only --tmpfs /tmp:rw,noexec,nosuid,size=128m --cap-drop ALL --security-opt no-new-privileges @limits -e IVR_EXECUTION_MODE=LAB_REAL_SIM -e REAL_CUSTOMER_CALL_ALLOWED=NO -e VIE_NEU_MAX_CONCURRENCY=1 -e VIE_NEU_ORT_THREADS=1 -e OPENBLAS_NUM_THREADS=1 -e OMP_NUM_THREADS=1 -e MKL_NUM_THREADS=1 -e "VIE_NEU_ALLOWED_VOICE_IDS=$voiceIds" --mount "type=bind,src=$modelPath,dst=/models,readonly" --mount "type=bind,src=$manifestPath,dst=/run/ivr-tts/voice-acceptance-manifest.json,readonly" --mount "type=bind,src=$textsPath,dst=/texts.json,readonly" --mount "type=bind,src=$probePath,dst=/probe.py,readonly" --mount "type=bind,src=$outPath,dst=/out" --entrypoint python $Image /probe.py --texts /texts.json --out /out --scope $EvidenceScope *> (Join-Path $outPath 'run.log')
$code=$LASTEXITCODE
$hostInfo['ended_utc']=[DateTimeOffset]::UtcNow.ToString('o')
$hostInfo['exit_code']=$code
$hostInfo|ConvertTo-Json -Depth 10|Set-Content -LiteralPath (Join-Path $outPath 'host.json') -Encoding utf8
Get-Content -LiteralPath (Join-Path $outPath 'run.log') -Tail 4
if($code -ne 0){throw "Load probe failed (exit $code). Retain its evidence."}
