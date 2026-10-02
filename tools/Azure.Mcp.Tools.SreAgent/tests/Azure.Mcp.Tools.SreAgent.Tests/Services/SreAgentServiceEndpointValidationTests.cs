// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Security;
using Azure.Mcp.Tools.SreAgent.Services;
using Azure.ResourceManager;
using Xunit;

namespace Azure.Mcp.Tools.SreAgent.Tests.Services;

/// <summary>
/// Verifies that <see cref="SreAgentService.ValidateDataPlaneEndpoint"/> only accepts SRE Agent
/// data-plane endpoints on the trusted <c>*.azuresre.ai</c> domain, preventing SSRF.
/// </summary>
public class SreAgentServiceEndpointValidationTests
{
    [Theory]
    [InlineData("https://agent1.azuresre.ai")]
    [InlineData("https://my-agent--abc.def.eastus2.azuresre.ai")]
    public void ValidateDataPlaneEndpoint_ValidHost_DoesNotThrow(string endpoint)
    {
        var exception = Record.Exception(() =>
            SreAgentService.ValidateDataPlaneEndpoint(new Uri(endpoint), ArmEnvironment.AzurePublicCloud));

        Assert.Null(exception);
    }

    [Theory]
    [InlineData("https://evil.example")]
    [InlineData("https://agent1.azuresre.ai.evil.example")]
    [InlineData("http://agent1.azuresre.ai")]
    [InlineData("https://127.0.0.1")]
    public void ValidateDataPlaneEndpoint_InvalidHost_ThrowsSecurityException(string endpoint)
    {
        Assert.Throws<SecurityException>(() =>
            SreAgentService.ValidateDataPlaneEndpoint(new Uri(endpoint), ArmEnvironment.AzurePublicCloud));
    }

    [Theory]
    [InlineData("china")]
    [InlineData("government")]
    public void ValidateDataPlaneEndpoint_SovereignCloud_ThrowsSecurityException(string cloud)
    {
        // SRE Agent is only offered in the public cloud, so the allow-list is empty for sovereign
        // clouds and validation must fail closed there.
        var armEnvironment = cloud == "china" ? ArmEnvironment.AzureChina : ArmEnvironment.AzureGovernment;

        Assert.Throws<SecurityException>(() =>
            SreAgentService.ValidateDataPlaneEndpoint(new Uri("https://agent1.azuresre.ai"), armEnvironment));
    }
}
