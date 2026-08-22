[CmdletBinding()]
param()

$ErrorActionPreference = "Stop"
$repositoryRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot "../..")).Path
$connectionVariable = "IVR_ACCOUNT_BOOTSTRAP_CONNECTION_STRING"
$connectionString = "Host=127.0.0.1;Port=55433;Database=ivr;Username=ivr"
$hadExistingConnection = Test-Path -LiteralPath "Env:$connectionVariable"
$existingConnection = [Environment]::GetEnvironmentVariable($connectionVariable)

Push-Location $repositoryRoot
try {
    [Environment]::SetEnvironmentVariable($connectionVariable, $connectionString)

    dotnet run `
        --project tools/Ivr.AccountBootstrap/Ivr.AccountBootstrap.csproj `
        --configuration Release `
        -- `
        --environment local
    if ($LASTEXITCODE -ne 0) {
        throw "Account seed failed with exit code $LASTEXITCODE."
    }
}
finally {
    if ($hadExistingConnection) {
        [Environment]::SetEnvironmentVariable($connectionVariable, $existingConnection)
    }
    else {
        Remove-Item -LiteralPath "Env:$connectionVariable" -ErrorAction SilentlyContinue
    }

    Pop-Location
}
