param(
    [string] $StorageAccountName,
    [string] $ResourceGroupName,
    [string] $SourcePath,
    [string] $SubscriptionId,
    [string] $TenantId,
    [bool] $Overwrite = $true,
    [hashtable] $DeploymentOutputs
)

$ErrorActionPreference = "Stop"

. "$PSScriptRoot/../../../eng/common/scripts/common.ps1"
. "$PSScriptRoot/../../../eng/scripts/helpers/TestResourcesHelpers.ps1"

$testSettings = New-TestSettings @PSBoundParameters -OutputPath $PSScriptRoot

# Login to Azure
az login --tenant $TenantId
az account set --subscription $SubscriptionId

# Enable static website hosting
az storage blob service-properties update `
    --account-name $StorageAccountName `
    --static-website `
    --index-document index.html `
    --404-document 404.html

# Upload files to $web container
az storage blob upload-batch `
    --account-name $StorageAccountName `
    --destination '$web' `
    --source $SourcePath `
    --overwrite $Overwrite `
    --auth-mode login `
    --subscription $SubscriptionId `
    --resource-group $ResourceGroupName