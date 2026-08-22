[CmdletBinding()]
param()

$ErrorActionPreference = "Stop"
$repositoryRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot "../..")).Path
$connectionString = "Host=127.0.0.1;Port=55433;Database=ivr;Username=ivr"

Push-Location $repositoryRoot
try {
    dotnet tool restore
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet tool restore failed with exit code $LASTEXITCODE."
    }

    dotnet tool run dotnet-ef database update `
        --project src/Ivr.Infrastructure/Ivr.Infrastructure.csproj `
        --startup-project src/Ivr.Infrastructure/Ivr.Infrastructure.csproj `
        --connection $connectionString
    if ($LASTEXITCODE -ne 0) {
        throw "Database migration failed with exit code $LASTEXITCODE."
    }
}
finally {
    Pop-Location
}
