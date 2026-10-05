// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Security;
using Azure.Mcp.Tools.Storage.Services;
using Azure.ResourceManager;
using Xunit;

namespace Azure.Mcp.Tools.Storage.Tests.Services;

public class StorageServiceEndpointValidationTests
{
    [Theory]
    [InlineData("https://mystorage.blob.core.windows.net/", "public")]
    [InlineData("https://mystorage.blob.core.chinacloudapi.cn/", "china")]
    [InlineData("https://mystorage.blob.core.usgovcloudapi.net/", "government")]
    public void ValidateBlobEndpoint_ValidCloudEndpoint_ReturnsEndpoint(string endpoint, string cloud)
    {
        var armEnvironment = GetArmEnvironment(cloud);
        var uri = new Uri(endpoint);

        Assert.Same(uri, StorageService.ValidateBlobEndpoint(uri, armEnvironment));
    }

    [Theory]
    [InlineData("https://evil.example/")]
    [InlineData("https://mystorage.blob.core.windows.net.evil.example/")]
    [InlineData("http://mystorage.blob.core.windows.net/")]
    [InlineData("https://mystorage.blob.core.chinacloudapi.cn/")]
    [InlineData("https://mystorage.table.core.windows.net/")]
    public void ValidateBlobEndpoint_InvalidPublicCloudEndpoint_ThrowsSecurityException(string endpoint)
    {
        Assert.Throws<SecurityException>(() =>
            StorageService.ValidateBlobEndpoint(new Uri(endpoint), ArmEnvironment.AzurePublicCloud));
    }

    [Theory]
    [InlineData("https://mystorage.table.core.windows.net/", "public")]
    [InlineData("https://mystorage.table.core.chinacloudapi.cn/", "china")]
    [InlineData("https://mystorage.table.core.usgovcloudapi.net/", "government")]
    public void ValidateTableEndpoint_ValidCloudEndpoint_ReturnsEndpoint(string endpoint, string cloud)
    {
        var armEnvironment = GetArmEnvironment(cloud);
        var uri = new Uri(endpoint);

        Assert.Same(uri, StorageService.ValidateTableEndpoint(uri, armEnvironment));
    }

    [Theory]
    [InlineData("https://evil.example/")]
    [InlineData("https://mystorage.table.core.windows.net.evil.example/")]
    [InlineData("http://mystorage.table.core.windows.net/")]
    [InlineData("https://mystorage.table.core.chinacloudapi.cn/")]
    [InlineData("https://mystorage.blob.core.windows.net/")]
    public void ValidateTableEndpoint_InvalidPublicCloudEndpoint_ThrowsSecurityException(string endpoint)
    {
        Assert.Throws<SecurityException>(() =>
            StorageService.ValidateTableEndpoint(new Uri(endpoint), ArmEnvironment.AzurePublicCloud));
    }

    private static ArmEnvironment GetArmEnvironment(string cloud) => cloud switch
    {
        "china" => ArmEnvironment.AzureChina,
        "government" => ArmEnvironment.AzureGovernment,
        _ => ArmEnvironment.AzurePublicCloud
    };
}
