[CmdletBinding()]
param(
    [Parameter(Mandatory = $true, Position = 0)]
    [ValidatePattern("^[A-Za-z][A-Za-z0-9_]{2,100}$")]
    [string] $Name
)

$ErrorActionPreference = "Stop"
$repositoryRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot "../..")).Path

Push-Location $repositoryRoot
try {
    dotnet tool restore
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet tool restore failed with exit code $LASTEXITCODE."
    }

    dotnet tool run dotnet-ef migrations add $Name `
        --project src/Ivr.Infrastructure/Ivr.Infrastructure.csproj `
        --startup-project src/Ivr.Infrastructure/Ivr.Infrastructure.csproj `
        --output-dir Persistence/Migrations
    if ($LASTEXITCODE -ne 0) {
        throw "Migration creation failed with exit code $LASTEXITCODE."
    }
}
finally {
    Pop-Location
}
