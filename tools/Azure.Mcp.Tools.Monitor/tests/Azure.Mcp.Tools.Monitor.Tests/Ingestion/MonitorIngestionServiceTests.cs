// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Security;
using Azure.Mcp.Tests.Helpers;
using Azure.Mcp.Tools.Monitor.Services;
using Azure.ResourceManager;
using Microsoft.Mcp.Core.Commands;
using Xunit;

namespace Azure.Mcp.Tools.Monitor.Tests.Ingestion;

public class MonitorIngestionServiceTests
{
    [Theory]
    [InlineData("https://my-dce.eastus-1.ingest.monitor.azure.com", "public")]
    [InlineData("https://my-dce.chinanorth3-1.ingest.monitor.azure.cn", "china")]
    [InlineData("https://my-dce.usgovvirginia-1.ingest.monitor.azure.us", "government")]
    public void CreateAndValidateEndpoint_ValidCloudHost_ReturnsEndpoint(string endpoint, string cloud)
    {
        var azureService = AzureServiceTestHelpers.CreateAzureService(armEnvironment: GetEnvironment(cloud));

        var result = MonitorIngestionService.CreateAndValidateEndpoint(azureService, endpoint);

        Assert.Equal(new Uri(endpoint), result);
    }

    [Theory]
    [InlineData("https://evil.example")]
    [InlineData("https://my-dce.eastus-1.ingest.monitor.azure.com.evil.example")]
    [InlineData("https://my-dce.chinanorth3-1.ingest.monitor.azure.cn")]
    [InlineData("https://my-dce.usgovvirginia-1.ingest.monitor.azure.us")]
    [InlineData("https://127.0.0.1")]
    public void CreateAndValidateEndpoint_DisallowedHost_ThrowsSecurityException(string endpoint)
    {
        var azureService = AzureServiceTestHelpers.CreateAzureService(armEnvironment: ArmEnvironment.AzurePublicCloud);

        Assert.Throws<SecurityException>(() => MonitorIngestionService.CreateAndValidateEndpoint(azureService, endpoint));
    }

    [Theory]
    [InlineData("not a url")]
    [InlineData("http://my-dce.eastus-1.ingest.monitor.azure.com")]
    [InlineData("https://user:pass@my-dce.eastus-1.ingest.monitor.azure.com")]
    [InlineData("https://my-dce.eastus-1.ingest.monitor.azure.com/path")]
    [InlineData("https://my-dce.eastus-1.ingest.monitor.azure.com/?q=1")]
    [InlineData("https://my-dce.eastus-1.ingest.monitor.azure.com/#frag")]
    public void CreateAndValidateEndpoint_ForbiddenUrlShape_ThrowsCommandValidationException(string endpoint)
    {
        var azureService = AzureServiceTestHelpers.CreateAzureService(armEnvironment: ArmEnvironment.AzurePublicCloud);

        Assert.Throws<CommandValidationException>(() => MonitorIngestionService.CreateAndValidateEndpoint(azureService, endpoint));
    }

    private static ArmEnvironment GetEnvironment(string cloud) => cloud switch
    {
        "china" => ArmEnvironment.AzureChina,
        "government" => ArmEnvironment.AzureGovernment,
        _ => ArmEnvironment.AzurePublicCloud
    };
}
