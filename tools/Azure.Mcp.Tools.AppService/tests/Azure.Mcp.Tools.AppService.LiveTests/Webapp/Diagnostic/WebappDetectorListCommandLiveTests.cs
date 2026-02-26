// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Tests.Client;
using Azure.Mcp.Tests.Client.Helpers;
using Azure.Mcp.Tests.Generated.Models;
using Azure.Mcp.Tools.AppService.Commands;
using Xunit;

namespace Azure.Mcp.Tools.AppService.LiveTests.Webapp.Diagnostic;

[Trait("Command", "WebappDetectorListCommand")]
public class WebappDetectorListCommandLiveTests(ITestOutputHelper output, TestProxyFixture fixture, LiveServerFixture liveServerFixture)
    : RecordedCommandTestsBase(output, fixture, liveServerFixture)
{
    public override List<BodyKeySanitizer> BodyKeySanitizers =>
    [
        ..base.BodyKeySanitizers,
        new BodyKeySanitizer(new BodyKeySanitizerBody("$.properties.selfLink")),
        new BodyKeySanitizer(new BodyKeySanitizerBody("$.properties.customDomainVerificationId")),
        new BodyKeySanitizer(new BodyKeySanitizerBody("$.properties.inboundIpAddress")),
        new BodyKeySanitizer(new BodyKeySanitizerBody("$.properties.possibleInboundIpAddresses")),
        new BodyKeySanitizer(new BodyKeySanitizerBody("$.properties.inboundIpv6Address")),
        new BodyKeySanitizer(new BodyKeySanitizerBody("$.properties.possibleInboundIpv6Addresses")),
        new BodyKeySanitizer(new BodyKeySanitizerBody("$.properties.ftpsHostName")),
        new BodyKeySanitizer(new BodyKeySanitizerBody("$.properties.outboundIpAddresses")),
        new BodyKeySanitizer(new BodyKeySanitizerBody("$.properties.possibleOutboundIpAddresses")),
        new BodyKeySanitizer(new BodyKeySanitizerBody("$.properties.outboundIpv6Addresses")),
        new BodyKeySanitizer(new BodyKeySanitizerBody("$.properties.possibleOutboundIpv6Addresses")),
        new BodyKeySanitizer(new BodyKeySanitizerBody("$.properties.homeStamp")),
    ];

    [Fact]
    public async Task ExecuteAsync_DetectorsList_ReturnsDetectors()
    {
        var webappName = RegisterOrRetrieveDeploymentOutputVariable("webappName", "WEBAPPNAME");
        webappName = TestMode == Tests.Helpers.TestMode.Playback ? "Sanitized-webapp" : webappName;
        var resourceGroupName = RegisterOrRetrieveVariable("resourceGroupName", Settings.ResourceGroupName);

        var result = await CallToolAsync(
            "appservice_webapp_diagnostic_list",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", resourceGroupName },
                { "app", webappName }
            });

        var detectorsResult = JsonSerializer.Deserialize(result.Value, AppServiceJsonContext.Default.WebappDetectorListResult);
        Assert.NotNull(detectorsResult);
        Assert.NotEmpty(detectorsResult.WebappDetectors);
    }
}
