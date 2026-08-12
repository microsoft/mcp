param(
    [string] $ResourceGroupName
)

$ErrorActionPreference = 'Stop'

$resourceGroup = Get-AzResourceGroup -Name $ResourceGroupName -ErrorAction SilentlyContinue
if ($null -eq $resourceGroup) {
    return
}

$uniqueSuffix = Get-AzResourceGroupDeployment -ResourceGroupName $ResourceGroupName |
    Sort-Object Timestamp -Descending |
    ForEach-Object {
        $outputs = $_.Outputs
        if ($outputs -and $outputs.ContainsKey('serviceGroupName')) {
            return @(
                $outputs['serviceGroupName'].Value,
                $outputs['lifecycleServiceGroupName'].Value
            )
        }
    } |
    Select-Object -First 1

foreach ($serviceGroupName in $uniqueSuffix) {
    if ([string]::IsNullOrWhiteSpace($serviceGroupName)) {
        continue
    }

    $path = "/providers/Microsoft.Management/serviceGroups/$serviceGroupName`?api-version=2024-02-01-preview"
    Write-Host "DELETE $path"
    $response = Invoke-AzRestMethod -Method DELETE -Path $path
    if ($response.StatusCode -notin @(200, 202, 204, 404)) {
        throw "DELETE $path failed with status $($response.StatusCode): $($response.Content)"
    }
}