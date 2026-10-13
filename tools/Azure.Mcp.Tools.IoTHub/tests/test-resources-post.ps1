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

Write-Host "Setting up IoT Hub test devices..."

$iotHubName = $DeploymentOutputs['IOTHUB_NAME']
Write-Host "IoT Hub Name: $iotHubName"

# Ensure Azure IoT CLI extension is installed for `az iot` commands
try { az extension show --name azure-iot | Out-Null } catch { az extension add --name azure-iot | Out-Null }

# Create test devices for device registry tests
$testDevices = @('test-device-1', 'test-device-2', 'test-device-3')
foreach ($deviceId in $testDevices) {
    try {
        # Create device identity
        az iot hub device-identity create --device-id $deviceId --hub-name $iotHubName --auth-type key
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
        
        az iot hub device-twin update --device-id $deviceId --hub-name $iotHubName --set "$twinPatch"
        Write-Host "Updated device twin for: $deviceId"
    }
    catch {
        Write-Warning "Failed to create/update device ${deviceId}: $_"
    }
}

Write-Host "IoT Hub test setup complete"



