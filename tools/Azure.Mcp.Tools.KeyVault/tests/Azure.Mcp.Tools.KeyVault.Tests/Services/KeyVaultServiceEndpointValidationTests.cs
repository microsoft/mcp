// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Security;
using Azure.Mcp.Tools.KeyVault.Services;
using Azure.ResourceManager;
using Xunit;

namespace Azure.Mcp.Tools.KeyVault.Tests.Services;

public class KeyVaultServiceEndpointValidationTests
{
    [Theory]
    [InlineData("https://vault.vault.azure.net/", "public")]
    [InlineData("https://vault.vault.azure.cn/", "china")]
    [InlineData("https://vault.vault.usgovcloudapi.net/", "government")]
    [InlineData("https://vault.vault.microsoftazure.de/", "germany")]
    public void ValidateVaultEndpoint_ValidCloudEndpoint_ReturnsEndpoint(string endpoint, string cloud)
    {
        var armEnvironment = cloud switch
        {
            "china" => ArmEnvironment.AzureChina,
            "government" => ArmEnvironment.AzureGovernment,
            "germany" => ArmEnvironment.AzureGermany,
            _ => ArmEnvironment.AzurePublicCloud
        };
        var uri = new Uri(endpoint);

        Assert.Same(uri, KeyVaultService.ValidateVaultEndpoint(uri, armEnvironment));
    }

    [Theory]
    [InlineData("https://evil.example/")]
    [InlineData("https://vault.vault.azure.net.evil.example/")]
    [InlineData("http://vault.vault.azure.net/")]
    [InlineData("https://vault.vault.azure.cn/")]
    [InlineData("https://hsm.managedhsm.azure.net/")]
    public void ValidateVaultEndpoint_InvalidPublicCloudEndpoint_ThrowsSecurityException(string endpoint)
    {
        Assert.Throws<SecurityException>(() =>
            KeyVaultService.ValidateVaultEndpoint(
                new Uri(endpoint),
                ArmEnvironment.AzurePublicCloud));
    }

    [Theory]
    [InlineData("https://hsm.managedhsm.azure.net/", "public")]
    [InlineData("https://hsm.managedhsm.azure.cn/", "china")]
    [InlineData("https://hsm.managedhsm.usgovcloudapi.net/", "government")]
    [InlineData("https://hsm.managedhsm.microsoftazure.de/", "germany")]
    public void ValidateManagedHsmEndpoint_ValidCloudEndpoint_ReturnsEndpoint(string endpoint, string cloud)
    {
        var armEnvironment = cloud switch
        {
            "china" => ArmEnvironment.AzureChina,
            "government" => ArmEnvironment.AzureGovernment,
            "germany" => ArmEnvironment.AzureGermany,
            _ => ArmEnvironment.AzurePublicCloud
        };
        var uri = new Uri(endpoint);

        Assert.Same(uri, KeyVaultService.ValidateManagedHsmEndpoint(uri, armEnvironment));
    }

    [Theory]
    [InlineData("https://evil.example/")]
    [InlineData("https://hsm.managedhsm.azure.net.evil.example/")]
    [InlineData("http://hsm.managedhsm.azure.net/")]
    [InlineData("https://hsm.managedhsm.azure.cn/")]
    [InlineData("https://vault.vault.azure.net/")]
    public void ValidateManagedHsmEndpoint_InvalidPublicCloudEndpoint_ThrowsSecurityException(string endpoint)
    {
        Assert.Throws<SecurityException>(() =>
            KeyVaultService.ValidateManagedHsmEndpoint(
                new Uri(endpoint),
                ArmEnvironment.AzurePublicCloud));
    }
}
