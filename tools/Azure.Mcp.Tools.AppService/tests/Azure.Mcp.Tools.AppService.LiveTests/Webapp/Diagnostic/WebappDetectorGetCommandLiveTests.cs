// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Tests.Client;
using Azure.Mcp.Tests.Client.Helpers;
using Azure.Mcp.Tests.Generated.Models;
using Azure.Mcp.Tools.AppService.Commands;
using Azure.Mcp.Tools.AppService.Models;
using Xunit;

namespace Azure.Mcp.Tools.AppService.LiveTests.Webapp.Diagnostic;

[Trait("Command", "WebappDetectorGetCommand")]
public class WebappDetectorGetCommandLiveTests(ITestOutputHelper output, TestProxyFixture fixture, LiveServerFixture liveServerFixture)
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

    [Fact(Skip = "Need to figure out test resource deployment.")]
    public async Task ExecuteAsync_DetectorsList_ReturnsDetectors()
    {
        var webappName = RegisterOrRetrieveDeploymentOutputVariable("webappName", "WEBAPPNAME");
        webappName = TestMode == Tests.Helpers.TestMode.Playback ? "Sanitized-webapp" : webappName;
        var resourceGroupName = RegisterOrRetrieveVariable("resourceGroupName", Settings.ResourceGroupName);

        await ListDetectorsAsync(Settings.SubscriptionId, resourceGroupName, webappName);
    }

    [Fact(Skip = "Need to figure out test resource deployment.")]
    public async Task ExecuteAsync_DetectorGet_ReturnsExpectedDetector()
    {
        var webappName = RegisterOrRetrieveDeploymentOutputVariable("webappName", "WEBAPPNAME");
        webappName = TestMode == Tests.Helpers.TestMode.Playback ? "Sanitized-webapp" : webappName;
        var resourceGroupName = RegisterOrRetrieveVariable("resourceGroupName", Settings.ResourceGroupName);

        var (webappDetectors, categoryName) = await ListDetectorsAsync(Settings.SubscriptionId, resourceGroupName, webappName);
        var detectorName = webappDetectors[0].Name;

        var result = await CallToolAsync(
            "appservice_webapp_diagnostic_get-detector",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", resourceGroupName },
                { "app", webappName },
                { "diagnostic-category", categoryName },
                { "detector-name", detectorName }
            });

        var detectorResult = JsonSerializer.Deserialize(result.Value, AppServiceJsonContext.Default.WebappDetectorGetResult);
        Assert.NotNull(detectorResult);
        Assert.Single(detectorResult.WebappDetectors);
        Assert.Equal(detectorName, detectorResult.WebappDetectors[0].Name);
    }

    private async Task<(List<WebappDetectorDetails>, string)> ListDetectorsAsync(string subscription, string resourceGroupName, string webappName)
    {
        var result = await CallToolAsync(
            "appservice_webapp_diagnostic_get-category",
            new()
            {
                { "subscription", subscription },
                { "resource-group", resourceGroupName },
                { "app", webappName }
            });

        var getResult = JsonSerializer.Deserialize(result.Value, AppServiceJsonContext.Default.WebappDiagnosticCategoryGetResult);
        Assert.NotNull(getResult);
        Assert.NotEmpty(getResult.WebappDiagnosticCategories);

        var categoryName = getResult.WebappDiagnosticCategories[0].Name;

        result = await CallToolAsync(
            "appservice_webapp_diagnostic_get-detector",
            new()
            {
                { "subscription", subscription },
                { "resource-group", resourceGroupName },
                { "app", webappName },
                { "diagnostic-category", categoryName }
            });

        var detectorsResult = JsonSerializer.Deserialize(result.Value, AppServiceJsonContext.Default.WebappDetectorGetResult);
        Assert.NotNull(detectorsResult);
        Assert.NotEmpty(detectorsResult.WebappDetectors);

        return (detectorsResult.WebappDetectors, categoryName);
    }
}
