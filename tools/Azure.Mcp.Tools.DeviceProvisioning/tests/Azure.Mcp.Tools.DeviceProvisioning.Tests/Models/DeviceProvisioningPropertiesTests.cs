// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Tools.DeviceProvisioning.Commands;
using Xunit;

namespace Azure.Mcp.Tools.DeviceProvisioning.Tests.Models;

public class DeviceProvisioningPropertiesTests
{
    [Fact]
    public void Deserialize_IgnoresSecretsAndPreservesSafeLinkedHubMetadata()
    {
        const string json = """
            {
              "state": "Active",
              "authorizationPolicies": [
                {
                  "keyName": "provisioningserviceowner",
                  "primaryKey": "secret-primary",
                  "secondaryKey": "secret-secondary"
                }
              ],
              "iotHubs": [
                {
                  "name": "hub1.azure-devices.net",
                  "location": "eastus",
                  "applyAllocationPolicy": true,
                  "allocationWeight": 1,
                  "connectionString": "HostName=hub1.azure-devices.net;SharedAccessKey=secret"
                }
              ]
            }
            """;

        var properties = JsonSerializer.Deserialize(
            json,
            DeviceProvisioningJsonContext.Default.DeviceProvisioningProperties);

        Assert.NotNull(properties);
        Assert.Equal("Active", properties.State);
        var linkedHub = Assert.Single(properties.IotHubs!);
        Assert.Equal("hub1.azure-devices.net", linkedHub.Name);
        Assert.Equal("eastus", linkedHub.Location);

        var serialized = JsonSerializer.Serialize(
            properties,
            DeviceProvisioningJsonContext.Default.DeviceProvisioningProperties);
        Assert.DoesNotContain("authorizationPolicies", serialized);
        Assert.DoesNotContain("primaryKey", serialized);
        Assert.DoesNotContain("connectionString", serialized);
        Assert.DoesNotContain("secret", serialized);
    }
}
