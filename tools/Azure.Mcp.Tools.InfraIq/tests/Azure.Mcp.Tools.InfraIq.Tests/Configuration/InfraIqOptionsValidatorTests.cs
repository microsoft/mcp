// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.InfraIq.Configuration;
using Microsoft.Extensions.Options;
using Xunit;

namespace Azure.Mcp.Tools.InfraIq.Tests.Configuration;

public class InfraIqOptionsValidatorTests
{
    private readonly InfraIqOptionsValidator _validator = new();

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("https://eastus2euap.management.azure.com")]
    [InlineData("https://eastus2euap.management.azure.com/")]
    [InlineData("HTTPS://EastUS2EUAP.Management.Azure.com")]
    [InlineData("https://management.azure.com")]
    [InlineData("https://Management.Azure.com/")]
    public void Validate_Succeeds_ForUnsetOrApprovedOrigin(string? origin)
    {
        var result = _validator.Validate(null, new InfraIqOptions { ArmIngressOrigin = origin });

        Assert.True(result.Succeeded);
    }

    [Theory]
    [InlineData("http://eastus2euap.management.azure.com")]
    [InlineData("https://user:pass@eastus2euap.management.azure.com")]
    [InlineData("https://eastus2euap.management.azure.com@evil.example.com")]
    [InlineData("https://eastus2euap.management.azure.com/subscriptions")]
    [InlineData("https://eastus2euap.management.azure.com//")]
    [InlineData("https://eastus2euap.management.azure.com/?x=1")]
    [InlineData("https://eastus2euap.management.azure.com?x=1")]
    [InlineData("https://eastus2euap.management.azure.com/#fragment")]
    [InlineData("http://management.azure.com")]
    [InlineData("https://management.azure.com/subscriptions")]
    [InlineData("https://management.azure.com:8443")]
    [InlineData("https://management.azure.com@evil.example.com")]
    [InlineData("https://management.azure.com.evil.example.com")]
    [InlineData("https://centraluseuap.management.azure.com")]
    [InlineData("https://management.chinacloudapi.cn")]
    [InlineData("https://management.usgovcloudapi.net")]
    [InlineData("https://eastus2euap.management.azure.com.evil.example.com")]
    [InlineData("https://evil.example.com")]
    [InlineData("https://eastus2euap.management.azure.com:8443")]
    [InlineData("https://eastus2euap.management.azure.com:443x")]
    [InlineData("https://eastus2euap.management.azure.com%2f.evil.example.com")]
    [InlineData("https://eastus2euap%2emanagement.azure.com")]
    [InlineData("https://eastus2euap.management.azure.com\\@evil.example.com")]
    [InlineData("//eastus2euap.management.azure.com")]
    [InlineData("eastus2euap.management.azure.com")]
    [InlineData(" https://eastus2euap.management.azure.com")]
    [InlineData("https://eastus2euap.management.azure.com\n")]
    public void Validate_Fails_ForUnapprovedOrigin(string origin)
    {
        var result = _validator.Validate(null, new InfraIqOptions { ArmIngressOrigin = origin });

        Assert.True(result.Failed);
        Assert.DoesNotContain("evil", result.FailureMessage);
    }

    [Theory]
    [InlineData("https://EastUS2EUAP.management.azure.com/", InfraIqArmIngress.CanaryOrigin)]
    [InlineData("https://Management.Azure.com", InfraIqArmIngress.DefaultOrigin)]
    public void TryParseOrigin_ReturnsCanonicalOrigin_ForApprovedValue(string configured, string expected)
    {
        var parsed = InfraIqArmIngress.TryParseOrigin(configured, out var uri, out var error);

        Assert.True(parsed);
        Assert.Null(error);
        Assert.Equal(expected + "/", uri!.AbsoluteUri);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void NormalizeConfiguredOrigin_MapsAbsentOrBlankToAutomaticRouting(string? configured)
    {
        Assert.Null(InfraIqArmIngress.NormalizeConfiguredOrigin(configured));
    }

    [Theory]
    [InlineData("https://evil.example.com")]
    [InlineData("http://eastus2euap.management.azure.com")]
    [InlineData(" https://eastus2euap.management.azure.com")]
    public void NormalizeConfiguredOrigin_PreservesNonBlankValuesSoValidationStillRejectsThem(string configured)
    {
        var resolved = InfraIqArmIngress.NormalizeConfiguredOrigin(configured);

        Assert.Equal(configured, resolved);
        Assert.True(_validator.Validate(null, new InfraIqOptions { ArmIngressOrigin = resolved }).Failed);
    }

    [Fact]
    public void TryParseOrigin_Fails_WhenOriginMissing()
    {
        var parsed = InfraIqArmIngress.TryParseOrigin(null, out var uri, out var error);

        Assert.False(parsed);
        Assert.Null(uri);
        Assert.Equal(InfraIqArmIngress.MissingOriginMessage, error);
    }
}
