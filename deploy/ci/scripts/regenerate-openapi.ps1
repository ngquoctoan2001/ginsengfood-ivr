$ErrorActionPreference = "Stop"

$repositoryRoot = Resolve-Path (Join-Path $PSScriptRoot "../../..")
Push-Location $repositoryRoot
try {
    dotnet tool restore
    if ($LASTEXITCODE -ne 0) { throw "dotnet tool restore failed." }

    dotnet nswag openapi2csclient `
        /Input:specs/api/openapi/ivr-order-confirmation.v1.yaml `
        /Output:src/Ivr.Contracts/Generated/IvrServer/V1/IvrServerModels.g.cs `
        /Namespace:Ivr.Contracts.Generated.IvrServer.V1 `
        /ClassName:IvrServerV1Client `
        /GenerateClientClasses:false `
        /GenerateDtoTypes:true `
        /JsonLibrary:SystemTextJson `
        /JsonLibraryVersion:10.0 `
        /GenerateNullableReferenceTypes:true `
        /GenerateOptionalPropertiesAsNullable:true `
        /UseRequiredKeyword:true `
        /WriteAccessor:init `
        /NewLineBehavior:LF
    if ($LASTEXITCODE -ne 0) { throw "IVR server DTO generation failed." }

    dotnet nswag openapi2csclient `
        /Input:specs/api/openapi/order-core-ivr-callback.target-v1.yaml `
        /Output:src/Ivr.Contracts/Generated/SalesTarget/V1/SalesTargetV1Client.g.cs `
        /Namespace:Ivr.Contracts.Generated.SalesTarget.V1 `
        /ClassName:SalesTargetV1Client `
        /GenerateClientClasses:true `
        /GenerateClientInterfaces:true `
        /InjectHttpClient:true `
        /DisposeHttpClient:false `
        /UseBaseUrl:true `
        /GenerateBaseUrlProperty:true `
        /OperationGenerationMode:SingleClientFromOperationId `
        /JsonLibrary:SystemTextJson `
        /JsonLibraryVersion:10.0 `
        /GenerateNullableReferenceTypes:true `
        /GenerateOptionalPropertiesAsNullable:true `
        /UseRequiredKeyword:true `
        /WriteAccessor:init `
        /NewLineBehavior:LF
    if ($LASTEXITCODE -ne 0) { throw "Sales Target V1 client generation failed." }

    Write-Output "OPENAPI_CODEGEN_COMPLETE=YES"
}
finally {
    Pop-Location
}
