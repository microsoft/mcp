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

[Trait("Command", "WebappAnalysisGetCommand")]
public class WebappAnalysisGetCommandLiveTests(ITestOutputHelper output, TestProxyFixture fixture, LiveServerFixture liveServerFixture)
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
    public async Task ExecuteAsync_AnalysesList_ReturnsAnalyses()
    {
        var webappName = RegisterOrRetrieveDeploymentOutputVariable("webappName", "WEBAPPNAME");
        webappName = TestMode == Tests.Helpers.TestMode.Playback ? "Sanitized-webapp" : webappName;
        var resourceGroupName = RegisterOrRetrieveVariable("resourceGroupName", Settings.ResourceGroupName);

        await ListAnalysesAsync(Settings.SubscriptionId, resourceGroupName, webappName);
    }

    [Fact(Skip = "Need to figure out test resource deployment.")]
    public async Task ExecuteAsync_AnalysisGet_ReturnsExpectedAnalysis()
    {
        var webappName = RegisterOrRetrieveDeploymentOutputVariable("webappName", "WEBAPPNAME");
        webappName = TestMode == Tests.Helpers.TestMode.Playback ? "Sanitized-webapp" : webappName;
        var resourceGroupName = RegisterOrRetrieveVariable("resourceGroupName", Settings.ResourceGroupName);

        var (webappAnalyses, categoryName) = await ListAnalysesAsync(Settings.SubscriptionId, resourceGroupName, webappName);
        var analysisName = webappAnalyses[0].Name;

        var result = await CallToolAsync(
            "appservice_webapp_diagnostic_get-analysis",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", resourceGroupName },
                { "app", webappName },
                { "diagnostic-category", categoryName },
                { "analysis-name", analysisName }
            });

        var analysisResult = JsonSerializer.Deserialize(result.Value, AppServiceJsonContext.Default.WebappAnalysisGetResult);
        Assert.NotNull(analysisResult);
        Assert.Single(analysisResult.WebappAnalyses);
        Assert.Equal(analysisName, analysisResult.WebappAnalyses[0].Name);
    }

    private async Task<(List<WebappAnalysisDetails>, string)> ListAnalysesAsync(string subscription, string resourceGroupName, string webappName)
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
            "appservice_webapp_diagnostic_get-analysis",
            new()
            {
                { "subscription", subscription },
                { "resource-group", resourceGroupName },
                { "app", webappName },
                { "diagnostic-category", categoryName }
            });

        var analysesResult = JsonSerializer.Deserialize(result.Value, AppServiceJsonContext.Default.WebappAnalysisGetResult);
        Assert.NotNull(analysesResult);
        Assert.NotEmpty(analysesResult.WebappAnalyses);

        return (analysesResult.WebappAnalyses, categoryName);
    }
}
