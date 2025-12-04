// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Tools.Dps.Services;
using Xunit;

namespace Azure.Mcp.Tools.Dps.UnitTests.Services;

public class DpsServiceTests
{
    [Theory]
    [InlineData("/subscriptions/sub1/resourceGroups/test-rg/providers/Microsoft.Devices/provisioningServices/test-dps", "test-rg")]
    [InlineData("/subscriptions/sub1/resourceGroups/my-rg/providers/Microsoft.Devices/provisioningServices/dps-instance", "my-rg")]
    [InlineData("/resourceGroups/rg-name/providers/Microsoft.Devices/provisioningServices/instance", "rg-name")]
    [InlineData("", "")]
    [InlineData("/invalid/path", "")]
    public void ExtractResourceGroupFromId_ExtractsCorrectly(string id, string expected)
    {
        // This tests the private method indirectly through ConvertToDpsInstanceInfo
        var jsonElement = JsonDocument.Parse($$"""
        {
            "id": "{{id}}",
            "name": "test-dps",
            "location": "eastus"
        }
        """).RootElement;

        var result = InvokeConvertToDpsInstanceInfo(jsonElement);

        Assert.Equal(expected, result.ResourceGroup);
    }

    [Fact]
    public void ConvertToDpsInstanceInfo_ParsesBasicProperties()
    {
        // Arrange
        var jsonElement = JsonDocument.Parse("""
        {
            "id": "/subscriptions/sub1/resourceGroups/rg1/providers/Microsoft.Devices/provisioningServices/test-dps",
            "name": "test-dps",
            "location": "eastus"
        }
        """).RootElement;

        // Act
        var result = InvokeConvertToDpsInstanceInfo(jsonElement);

        // Assert
        Assert.Equal("test-dps", result.Name);
        Assert.Equal("/subscriptions/sub1/resourceGroups/rg1/providers/Microsoft.Devices/provisioningServices/test-dps", result.Id);
        Assert.Equal("rg1", result.ResourceGroup);
        Assert.Equal("eastus", result.Location);
    }

    [Fact]
    public void ConvertToDpsInstanceInfo_ParsesAllProperties()
    {
        // Arrange
        var jsonElement = JsonDocument.Parse("""
        {
            "id": "/subscriptions/sub1/resourceGroups/rg1/providers/Microsoft.Devices/provisioningServices/full-dps",
            "name": "full-dps",
            "location": "westus",
            "properties": {
                "provisioningState": "Succeeded",
                "serviceOperationsHostName": "full-dps.azure-devices-provisioning.net",
                "deviceProvisioningHostName": "global.azure-devices-provisioning.net",
                "idScope": "0ne00000001",
                "allocationPolicy": "Hashed",
                "state": "Active"
            },
            "sku": {
                "name": "S1",
                "tier": "Standard",
                "capacity": 1
            }
        }
        """).RootElement;

        // Act
        var result = InvokeConvertToDpsInstanceInfo(jsonElement);

        // Assert
        Assert.Equal("full-dps", result.Name);
        Assert.Equal("westus", result.Location);
        Assert.Equal("Succeeded", result.ProvisioningState);
        Assert.Equal("full-dps.azure-devices-provisioning.net", result.ServiceOperationsHostName);
        Assert.Equal("global.azure-devices-provisioning.net", result.DeviceProvisioningHostName);
        Assert.Equal("0ne00000001", result.IdScope);
        Assert.Equal("Hashed", result.AllocationPolicy);
        Assert.Equal("Active", result.State);
        Assert.Equal("S1", result.Sku);
    }

    [Fact]
    public void ConvertToDpsInstanceInfo_HandlesPartialProperties()
    {
        // Arrange
        var jsonElement = JsonDocument.Parse("""
        {
            "id": "/subscriptions/sub1/resourceGroups/rg1/providers/Microsoft.Devices/provisioningServices/partial-dps",
            "name": "partial-dps",
            "location": "centralus",
            "properties": {
                "provisioningState": "Creating",
                "idScope": "0ne00000002"
            }
        }
        """).RootElement;

        // Act
        var result = InvokeConvertToDpsInstanceInfo(jsonElement);

        // Assert
        Assert.Equal("partial-dps", result.Name);
        Assert.Equal("centralus", result.Location);
        Assert.Equal("Creating", result.ProvisioningState);
        Assert.Equal("0ne00000002", result.IdScope);
        Assert.Null(result.ServiceOperationsHostName);
        Assert.Null(result.DeviceProvisioningHostName);
        Assert.Null(result.AllocationPolicy);
        Assert.Null(result.State);
        Assert.Null(result.Sku);
    }

    [Fact]
    public void ConvertToDpsInstanceInfo_HandlesMinimalJson()
    {
        // Arrange
        var jsonElement = JsonDocument.Parse("""
        {
            "id": "/subscriptions/sub1/resourceGroups/rg1/providers/Microsoft.Devices/provisioningServices/minimal-dps",
            "name": "minimal-dps",
            "location": "northeurope"
        }
        """).RootElement;

        // Act
        var result = InvokeConvertToDpsInstanceInfo(jsonElement);

        // Assert
        Assert.Equal("minimal-dps", result.Name);
        Assert.Equal("northeurope", result.Location);
        Assert.Null(result.ProvisioningState);
        Assert.Null(result.Sku);
    }

    [Fact]
    public void ConvertToDpsInstanceInfo_HandlesEmptyProperties()
    {
        // Arrange
        var jsonElement = JsonDocument.Parse("""
        {
            "id": "/subscriptions/sub1/resourceGroups/rg1/providers/Microsoft.Devices/provisioningServices/empty-props-dps",
            "name": "empty-props-dps",
            "location": "eastus2",
            "properties": {},
            "sku": {}
        }
        """).RootElement;

        // Act
        var result = InvokeConvertToDpsInstanceInfo(jsonElement);

        // Assert
        Assert.Equal("empty-props-dps", result.Name);
        Assert.Equal("eastus2", result.Location);
        Assert.Null(result.ProvisioningState);
        Assert.Null(result.ServiceOperationsHostName);
        Assert.Null(result.Sku);
    }

    /// <summary>
    /// Helper method to invoke the private ConvertToDpsInstanceInfo method via reflection.
    /// </summary>
    private static Models.DpsInstanceInfo InvokeConvertToDpsInstanceInfo(JsonElement jsonElement)
    {
        var method = typeof(DpsService).GetMethod(
            "ConvertToDpsInstanceInfo",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

        if (method == null)
        {
            throw new InvalidOperationException("ConvertToDpsInstanceInfo method not found");
        }

        var result = method.Invoke(null, [jsonElement]);
        return (Models.DpsInstanceInfo)result!;
    }
}
