// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Security;
using Azure.Mcp.Tools.Acr.Services;
using Azure.ResourceManager;
using Xunit;

namespace Azure.Mcp.Tools.Acr.Tests.Services;

public class AcrServiceEndpointValidationTests
{
    [Theory]
    [InlineData("myregistry.azurecr.io", "https://myregistry.azurecr.io/")]
    public void CreateValidatedAcrEndpoint_PublicCloudHost_ReturnsEndpoint(string loginServer, string expectedEndpoint)
    {
        var endpoint = AcrService.CreateValidatedAcrEndpoint(loginServer, ArmEnvironment.AzurePublicCloud);

        Assert.Equal(expectedEndpoint, endpoint.AbsoluteUri);
    }

    [Theory]
    [InlineData("myregistry.azurecr.cn", "https://myregistry.azurecr.cn/")]
    public void CreateValidatedAcrEndpoint_ChinaCloudHost_ReturnsEndpoint(string loginServer, string expectedEndpoint)
    {
        var endpoint = AcrService.CreateValidatedAcrEndpoint(loginServer, ArmEnvironment.AzureChina);

        Assert.Equal(expectedEndpoint, endpoint.AbsoluteUri);
    }

    [Theory]
    [InlineData("evil.com")]
    [InlineData("myregistry.azurecr.io.evil.com")]
    [InlineData("10.0.0.1")]
    [InlineData("myregistry.azurecr.cn")]
    public void CreateValidatedAcrEndpoint_InvalidPublicCloudHost_ThrowsSecurityException(string loginServer)
    {
        Assert.Throws<SecurityException>(
            () => AcrService.CreateValidatedAcrEndpoint(loginServer, ArmEnvironment.AzurePublicCloud));
    }
}
