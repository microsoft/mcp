// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Security;
using Azure.Mcp.Tools.ConfidentialLedger.Services;
using Azure.ResourceManager;
using Xunit;

namespace Azure.Mcp.Tools.ConfidentialLedger.Tests.Services;

public class ConfidentialLedgerServiceEndpointValidationTests
{
    [Theory]
    [InlineData("https://myledger.confidential-ledger.azure.com/", "public")]
    [InlineData("https://myledger.confidential-ledger.azure.cn/", "china")]
    [InlineData("https://myledger.confidential-ledger.azure.us/", "government")]
    public void ValidateLedgerEndpoint_ValidCloudEndpoint_ReturnsEndpoint(string endpoint, string cloud)
    {
        var armEnvironment = cloud switch
        {
            "china" => ArmEnvironment.AzureChina,
            "government" => ArmEnvironment.AzureGovernment,
            _ => ArmEnvironment.AzurePublicCloud
        };
        var uri = new Uri(endpoint);

        Assert.Same(uri, ConfidentialLedgerService.ValidateLedgerEndpoint(uri, armEnvironment));
    }

    [Fact]
    public void ValidateLedgerEndpoint_GermanyCloud_ThrowsSecurityException()
    {
        // Confidential Ledger is not offered in the Germany cloud, so validation has no allowed suffix and fails.
        Assert.Throws<SecurityException>(() =>
            ConfidentialLedgerService.ValidateLedgerEndpoint(
                new Uri("https://myledger.confidential-ledger.azure.com/"),
                ArmEnvironment.AzureGermany));
    }

    [Theory]
    [InlineData("https://evil.example/")]
    [InlineData("https://myledger.confidential-ledger.azure.com.evil.example/")]
    [InlineData("http://myledger.confidential-ledger.azure.com/")] // non-HTTPS
    [InlineData("https://myledger.confidential-ledger.azure.cn/")] // wrong cloud suffix for the public allow-list
    public void ValidateLedgerEndpoint_InvalidPublicCloudEndpoint_ThrowsSecurityException(string endpoint)
    {
        Assert.Throws<SecurityException>(() =>
            ConfidentialLedgerService.ValidateLedgerEndpoint(
                new Uri(endpoint),
                ArmEnvironment.AzurePublicCloud));
    }
}
