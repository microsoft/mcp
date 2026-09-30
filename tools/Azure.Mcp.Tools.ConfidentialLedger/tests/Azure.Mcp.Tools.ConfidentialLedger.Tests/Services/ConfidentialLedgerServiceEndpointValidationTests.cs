// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Security;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tools.ConfidentialLedger.Services;
using Azure.ResourceManager;
using Microsoft.Mcp.Core.Services.Azure.Authentication;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.ConfidentialLedger.Tests.Services;

public class ConfidentialLedgerServiceEndpointValidationTests
{
    private static ConfidentialLedgerService CreateService(
        AzureCloudConfiguration.AzureCloud cloudType,
        ArmEnvironment armEnvironment)
    {
        var cloudConfiguration = Substitute.For<IAzureCloudConfiguration>();
        cloudConfiguration.CloudType.Returns(cloudType);
        cloudConfiguration.ArmEnvironment.Returns(armEnvironment);

        var azureService = Substitute.For<IAzureService>();
        azureService.CloudConfiguration.Returns(cloudConfiguration);

        return new ConfidentialLedgerService(azureService);
    }

    [Theory]
    [InlineData(AzureCloudConfiguration.AzureCloud.AzurePublicCloud, "https://myledger.confidential-ledger.azure.com/")]
    [InlineData(AzureCloudConfiguration.AzureCloud.AzureChinaCloud, "https://myledger.confidential-ledger.azure.cn/")]
    [InlineData(AzureCloudConfiguration.AzureCloud.AzureUSGovernmentCloud, "https://myledger.confidential-ledger.azure.us/")]
    public void GetValidatedLedgerUri_ValidCloud_ReturnsEndpoint(
        AzureCloudConfiguration.AzureCloud cloudType,
        string expected)
    {
        var armEnvironment = cloudType switch
        {
            AzureCloudConfiguration.AzureCloud.AzureChinaCloud => ArmEnvironment.AzureChina,
            AzureCloudConfiguration.AzureCloud.AzureUSGovernmentCloud => ArmEnvironment.AzureGovernment,
            _ => ArmEnvironment.AzurePublicCloud
        };
        var service = CreateService(cloudType, armEnvironment);

        var uri = service.GetValidatedLedgerUri("myledger");

        Assert.Equal(expected, uri.AbsoluteUri);
    }

    [Theory]
    [InlineData("bad.name")]
    [InlineData("has space")]
    [InlineData("under_score")]
    [InlineData("9ledger")] // must not start with a digit
    [InlineData("-ledger")] // must not start with a hyphen
    public void GetValidatedLedgerUri_InvalidLedgerName_ThrowsArgumentException(string ledgerName)
    {
        var service = CreateService(
            AzureCloudConfiguration.AzureCloud.AzurePublicCloud,
            ArmEnvironment.AzurePublicCloud);

        Assert.Throws<ArgumentException>(() => service.GetValidatedLedgerUri(ledgerName));
    }

    [Fact]
    public void GetValidatedLedgerUri_GermanyCloud_ThrowsSecurityException()
    {
        // Confidential Ledger is not offered in the Germany cloud; the allow-list has no Germany suffix, so
        // validation fails rather than implicitly accepting a public-cloud host.
        var service = CreateService(
            AzureCloudConfiguration.AzureCloud.AzurePublicCloud,
            ArmEnvironment.AzureGermany);

        Assert.Throws<SecurityException>(() => service.GetValidatedLedgerUri("myledger"));
    }
}
