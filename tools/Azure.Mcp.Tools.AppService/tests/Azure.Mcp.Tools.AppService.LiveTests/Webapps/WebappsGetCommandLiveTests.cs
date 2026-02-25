// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Tests.Client;
using Azure.Mcp.Tests.Client.Helpers;
using Azure.Mcp.Tests.Generated.Models;
using Azure.Mcp.Tools.AppService.Commands;
using Xunit;

namespace Azure.Mcp.Tools.AppService.LiveTests.Webapps;

[Trait("Command", "WebappsGetCommand")]
public class WebappsGetCommandLiveTests(ITestOutputHelper output, TestProxyFixture fixture, LiveServerFixture liveServerFixture) : RecordedCommandTestsBase(output, fixture, liveServerFixture)
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
    public async Task ExecuteAsync_SubscriptionList_ReturnsExpectedWebApp()
    {
        var webappName = RegisterOrRetrieveDeploymentOutputVariable("webappName", "WEBAPPNAME");
        var expectedWebappName = TestMode == Tests.Helpers.TestMode.Playback ? "Sanitized" : webappName;

        var result = await CallToolAsync(
            "appservice_webapps_get",
            new()
            {
                { "subscription", Settings.SubscriptionId }
            });

        var getResult = JsonSerializer.Deserialize(result.Value.ToString(), AppServiceJsonContext.Default.WebappsGetResult);
        Assert.NotNull(getResult);
        Assert.NotEmpty(getResult.WebappDetails);
        Assert.True(getResult.WebappDetails.Any(detail => detail.Name == expectedWebappName), $"Expected to find web app with name '{expectedWebappName}' in the results.");
    }

    [Fact]
    public async Task ExecuteAsync_ResourceGroupList_ReturnsExpectedWebApp()
    {
        var webappName = RegisterOrRetrieveDeploymentOutputVariable("webappName", "WEBAPPNAME");
        var expectedWebappName = TestMode == Tests.Helpers.TestMode.Playback ? "Sanitized" : webappName;
        var resourceGroupName = RegisterOrRetrieveVariable("resourceGroupName", Settings.ResourceGroupName);

        var result = await CallToolAsync(
            "appservice_webapps_get",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", resourceGroupName }
            });

        var getResult = JsonSerializer.Deserialize(result.Value.ToString(), AppServiceJsonContext.Default.WebappsGetResult);
        Assert.NotNull(getResult);
        Assert.NotEmpty(getResult.WebappDetails);
        Assert.True(getResult.WebappDetails.Any(detail => detail.Name == expectedWebappName), $"Expected to find web app with name '{expectedWebappName}' in the results.");
    }

    [Fact]
    public async Task ExecuteAsync_WebAppGet_ReturnsExpectedWebApp()
    {
        var webappName = RegisterOrRetrieveDeploymentOutputVariable("webappName", "WEBAPPNAME");
        var expectedWebappName = TestMode == Tests.Helpers.TestMode.Playback ? "Sanitized" : webappName;
        webappName = TestMode == Tests.Helpers.TestMode.Playback ? "Sanitized-" + webappName.Split('-')[1] : webappName;
        var resourceGroupName = RegisterOrRetrieveVariable("resourceGroupName", Settings.ResourceGroupName);

        var result = await CallToolAsync(
            "appservice_webapps_get",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", resourceGroupName },
                { "app", webappName }
            });

        var getResult = JsonSerializer.Deserialize(result.Value.ToString(), AppServiceJsonContext.Default.WebappsGetResult);
        Assert.NotNull(getResult);
        Assert.Single(getResult.WebappDetails);
        Assert.True(getResult.WebappDetails.All(detail => detail.Name == expectedWebappName), $"Expected to find a single web app with name '{expectedWebappName}' in the results.");
    }
}
