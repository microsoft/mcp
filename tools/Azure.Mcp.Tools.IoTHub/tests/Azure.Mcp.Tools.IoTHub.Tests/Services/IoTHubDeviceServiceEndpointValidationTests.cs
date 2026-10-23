// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Security;
using Azure.Mcp.Tests.Helpers;
using Azure.Mcp.Tools.IoTHub.Services;
using Azure.ResourceManager;
using Xunit;

namespace Azure.Mcp.Tools.IoTHub.Tests.Services;

public class IoTHubDeviceServiceEndpointValidationTests
{
    [Theory]
    [InlineData("hub.azure-devices.net", "public")]
    [InlineData("hub.service.azure-devices.net", "public")]
    [InlineData("hub.azure-devices.cn", "china")]
    [InlineData("hub.azure-devices.us", "government")]
    public void CreateValidatedDataPlaneUri_ValidCloudHost_ReturnsEndpoint(string hostname, string cloud)
    {
        var armEnvironment = cloud switch
        {
            "china" => ArmEnvironment.AzureChina,
            "government" => ArmEnvironment.AzureGovernment,
            _ => ArmEnvironment.AzurePublicCloud
        };

        Uri endpoint = IoTHubDeviceService.CreateValidatedDataPlaneUri(AzureServiceTestHelpers.CreateAzureService(armEnvironment: armEnvironment),
            hostname,
            "/devices?api-version=2021-04-12");

        Assert.Equal($"https://{hostname}/devices?api-version=2021-04-12", endpoint.AbsoluteUri);
    }

    [Theory]
    [InlineData("evil.example")]
    [InlineData("hub.azure-devices.net.evil.example")]
    [InlineData("hub.azure-devices.cn")]
    [InlineData("127.0.0.1")]
    public void CreateValidatedDataPlaneUri_InvalidPublicCloudHost_ThrowsSecurityException(string hostname)
    {
        Assert.Throws<SecurityException>(() =>
            IoTHubDeviceService.CreateValidatedDataPlaneUri(AzureServiceTestHelpers.CreateAzureService(armEnvironment: ArmEnvironment.AzurePublicCloud),
                hostname,
                "/devices?api-version=2021-04-12"));
    }
}
