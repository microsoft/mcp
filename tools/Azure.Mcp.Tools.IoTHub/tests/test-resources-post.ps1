param(
    [string] $TenantId,
    [string] $TestApplicationId,
    [string] $ResourceGroupName,
    [string] $BaseName,
    [hashtable] $DeploymentOutputs,
    [hashtable] $AdditionalParameters
)

$ErrorActionPreference = "Stop"

. "$PSScriptRoot/../../../eng/common/scripts/common.ps1"
. "$PSScriptRoot/../../../eng/scripts/helpers/TestResourcesHelpers.ps1"

$testSettings = New-TestSettings @PSBoundParameters -OutputPath $PSScriptRoot

function Get-PlainTextAccessToken {
    param([Parameter(Mandatory)] [string] $ResourceUrl)

    $token = (Get-AzAccessToken -ResourceUrl $ResourceUrl).Token
    if ($token -is [System.Security.SecureString]) {
        return [System.Net.NetworkCredential]::new('', $token).Password
    }

    return $token
}

function Invoke-IoTHubRegistryRequest {
    param(
        [Parameter(Mandatory)] [string] $Method,
        [Parameter(Mandatory)] [string] $Uri,
        [Parameter(Mandatory)] [string] $Body,
        [Parameter(Mandatory)] [string] $AccessToken,
        [int[]] $AcceptedStatusCodes = @()
    )

    $headers = @{ Authorization = "Bearer $AccessToken" }
    $maxAttempts = 30
    for ($attempt = 1; $attempt -le $maxAttempts; $attempt++) {
        try {
            Invoke-RestMethod -Method $Method -Uri $Uri -Headers $headers -ContentType 'application/json' -Body $Body | Out-Null
            return
        } catch {
            if ($_.Exception -isnot [Microsoft.PowerShell.Commands.HttpResponseException] -or $null -eq $_.Exception.Response) {
                throw
            }

            $statusCode = [int]$_.Exception.Response.StatusCode
            if ($AcceptedStatusCodes -contains $statusCode) {
                return
            }

            # The IoT Hub Data Contributor assignment from test-resources.bicep can take several minutes to take effect.
            $isTransient = $statusCode -eq 401 -or $statusCode -eq 403 -or $statusCode -eq 429 -or $statusCode -ge 500
            if (-not $isTransient -or $attempt -eq $maxAttempts) {
                throw
            }

            Write-Host "IoT Hub returned $statusCode for $Method $Uri (attempt $attempt of $maxAttempts); retrying in 20 seconds."
            Start-Sleep -Seconds 20
        }
    }
}

function Set-IoTHubRouting {
    param(
        [Parameter(Mandatory)] [string] $HubPath,
        [Parameter(Mandatory)] [object[]] $ServiceBusQueues,
        [Parameter(Mandatory)] [object[]] $ServiceBusTopics,
        [Parameter(Mandatory)] [object[]] $Routes
    )

    $maxAttempts = 30
    for ($attempt = 1; $attempt -le $maxAttempts; $attempt++) {
        $response = Invoke-AzRestMethod -Path $HubPath -Method GET
        if ($response.StatusCode -ne 200) {
            throw "Reading IoT Hub failed with status $($response.StatusCode): $($response.Content)"
        }

        # IoT Hub has no PATCH for properties, so write back the full resource with the routing added.
        $hub = $response.Content | ConvertFrom-Json -AsHashtable -Depth 64
        $hub.properties.routing.endpoints.serviceBusQueues = $ServiceBusQueues
        $hub.properties.routing.endpoints.serviceBusTopics = $ServiceBusTopics
        $hub.properties.routing.routes = $Routes

        $response = Invoke-AzRestMethod -Path $HubPath -Method PUT -Payload (ConvertTo-Json -InputObject $hub -Depth 64)
        if ($response.StatusCode -in 200, 201) {
            break
        }

        # IoT Hub rejects identity-based endpoints (IH400117) until the Service Bus Data Sender assignment from
        # test-resources.bicep takes effect, which can take several minutes.
        if ($response.StatusCode -ne 400 -or $response.Content -notmatch '400117' -or $attempt -eq $maxAttempts) {
            throw "Configuring IoT Hub routing failed with status $($response.StatusCode): $($response.Content)"
        }

        Write-Host "IoT Hub cannot authenticate to the routing endpoints yet (attempt $attempt of $maxAttempts); retrying in 20 seconds."
        Start-Sleep -Seconds 20
    }

    for ($poll = 1; $poll -le 60; $poll++) {
        $hub = (Invoke-AzRestMethod -Path $HubPath -Method GET).Content | ConvertFrom-Json -AsHashtable -Depth 64
        switch ($hub.properties.provisioningState) {
            'Succeeded' { return }
            'Failed' { throw "The IoT Hub routing update failed." }
        }
        Start-Sleep -Seconds 10
    }

    throw "Timed out waiting for the IoT Hub routing update to finish."
}

$iotHubName = $DeploymentOutputs['IOTHUB_NAME']
$iotHubHostName = $DeploymentOutputs['IOTHUB_HOST_NAME']
Write-Host "IoT Hub Name: $iotHubName"

Write-Host "Configuring IoT Hub routing endpoints..."

$routingEndpoint = @{
    endpointUri = $DeploymentOutputs['ROUTING_ENDPOINT_URI']
    subscriptionId = $testSettings.SubscriptionId
    resourceGroup = $ResourceGroupName
    authenticationType = 'identityBased'
    identity = @{ userAssignedIdentity = $DeploymentOutputs['ROUTING_IDENTITY_ID'] }
}
$routes = foreach ($endpointName in 'sbqueue-endpoint', 'sbtopic-endpoint') {
    @{
        name = $endpointName
        source = 'DeviceMessages'
        condition = 'true'
        endpointNames = @($endpointName)
        isEnabled = $true
    }
}
Set-IoTHubRouting `
    -HubPath "/subscriptions/$($testSettings.SubscriptionId)/resourceGroups/$ResourceGroupName/providers/Microsoft.Devices/IotHubs/$iotHubName`?api-version=2023-06-30" `
    -ServiceBusQueues @($routingEndpoint + @{ name = 'sbqueue-endpoint'; entityPath = $DeploymentOutputs['ROUTING_QUEUE_NAME'] }) `
    -ServiceBusTopics @($routingEndpoint + @{ name = 'sbtopic-endpoint'; entityPath = $DeploymentOutputs['ROUTING_TOPIC_NAME'] }) `
    -Routes @($routes)
Write-Host "Configured IoT Hub routing endpoints"

Write-Host "Setting up IoT Hub test devices..."

# Use the registry REST API with Microsoft Entra ID, as the tools do. The azure-iot CLI extension used previously
# cannot be installed on network-isolated pipeline agents.
$accessToken = Get-PlainTextAccessToken -ResourceUrl 'https://iothubs.azure.net'
$apiVersion = '2021-04-12'

# Create test devices for device registry tests
$testDevices = @('test-device-1', 'test-device-2', 'test-device-3')
foreach ($deviceId in $testDevices) {
    try {
        # Create device identity; 409 means it already exists from an earlier run against the same hub.
        $device = @{
            deviceId = $deviceId
            authentication = @{ type = 'sas' }
        } | ConvertTo-Json -Depth 10
        Invoke-IoTHubRegistryRequest -Method Put -Uri "https://$iotHubHostName/devices/$deviceId`?api-version=$apiVersion" `
            -Body $device -AccessToken $accessToken -AcceptedStatusCodes @(409)
        Write-Host "Created device: $deviceId"

        # Update device twin with test properties
        $twinPatch = @{
            properties = @{
                desired = @{
                    temperature = 72
                    location = "testlab"
                }
            }
            tags = @{
                environment = "test"
                deviceType = "sensor"
            }
        } | ConvertTo-Json -Depth 10
        Invoke-IoTHubRegistryRequest -Method Patch -Uri "https://$iotHubHostName/twins/$deviceId`?api-version=$apiVersion" `
            -Body $twinPatch -AccessToken $accessToken
        Write-Host "Updated device twin for: $deviceId"
    }
    catch {
        Write-Warning "Failed to create/update device ${deviceId}: $_"
    }
}

Write-Host "IoT Hub test setup complete"



