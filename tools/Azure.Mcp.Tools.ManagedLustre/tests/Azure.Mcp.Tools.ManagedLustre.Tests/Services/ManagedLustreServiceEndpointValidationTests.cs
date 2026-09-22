// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Security;
using Azure.Mcp.Tools.ManagedLustre.Services;
using Azure.ResourceManager;
using Xunit;

namespace Azure.Mcp.Tools.ManagedLustre.Tests.Services;

public class ManagedLustreServiceEndpointValidationTests
{
    [Theory]
    [InlineData("https://vault.vault.azure.net/keys/key/version", "public")]
    [InlineData("https://vault.vault.azure.cn/keys/key/version", "china")]
    [InlineData("https://vault.vault.usgovcloudapi.net/keys/key/version", "government")]
    [InlineData("https://vault.vault.microsoftazure.de/keys/key/version", "germany")]
    public void CreateValidatedKeyUri_ValidCloudEndpoint_ReturnsEndpoint(string endpoint, string cloud)
    {
        var armEnvironment = cloud switch
        {
            "china" => ArmEnvironment.AzureChina,
            "government" => ArmEnvironment.AzureGovernment,
            "germany" => ArmEnvironment.AzureGermany,
            _ => ArmEnvironment.AzurePublicCloud
        };

        Assert.Equal(
            endpoint,
            ManagedLustreService.CreateValidatedKeyUri(endpoint, armEnvironment).AbsoluteUri);
    }

    [Theory]
    [InlineData("https://evil.example/keys/key/version")]
    [InlineData("https://vault.vault.azure.net.evil.example/keys/key/version")]
    [InlineData("http://vault.vault.azure.net/keys/key/version")]
    [InlineData("https://vault.vault.azure.cn/keys/key/version")]
    public void CreateValidatedKeyUri_InvalidPublicCloudEndpoint_ThrowsSecurityException(string endpoint)
    {
        Assert.Throws<SecurityException>(() =>
            ManagedLustreService.CreateValidatedKeyUri(
                endpoint,
                ArmEnvironment.AzurePublicCloud));
    }
}
