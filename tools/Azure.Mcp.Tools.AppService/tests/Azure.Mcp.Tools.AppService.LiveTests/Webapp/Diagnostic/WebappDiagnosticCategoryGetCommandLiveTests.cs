// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Tests.Client;
using Azure.Mcp.Tests.Client.Helpers;
using Azure.Mcp.Tests.Generated.Models;
using Azure.Mcp.Tools.AppService.Commands;
using Xunit;

namespace Azure.Mcp.Tools.AppService.LiveTests.Webapp.Diagnostic;

[Trait("Command", "WebappDiagnosticCategoryGetCommand")]
public class WebappDiagnosticCategoryGetCommandLiveTests(ITestOutputHelper output, TestProxyFixture fixture, LiveServerFixture liveServerFixture)
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
    public async Task ExecuteAsync_DiagnosticCategoryList_ReturnsDiagnosticCategories()
    {
        var webappName = RegisterOrRetrieveDeploymentOutputVariable("webappName", "WEBAPPNAME");
        webappName = TestMode == Tests.Helpers.TestMode.Playback ? "Sanitized-webapp" : webappName;
        var resourceGroupName = RegisterOrRetrieveVariable("resourceGroupName", Settings.ResourceGroupName);

        var result = await CallToolAsync(
            "appservice_webapp_diagnostic_get-category",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", resourceGroupName },
                { "app", webappName }
            });

        var getResult = JsonSerializer.Deserialize(result.Value, AppServiceJsonContext.Default.WebappDiagnosticCategoryGetResult);
        Assert.NotNull(getResult);
        Assert.NotEmpty(getResult.WebappDiagnosticCategories);
    }

    [Fact(Skip = "Need to figure out test resource deployment.")]
    public async Task ExecuteAsync_DiagnosticCategoryGet_ReturnsExpectedDiagnosticCategory()
    {
        var webappName = RegisterOrRetrieveDeploymentOutputVariable("webappName", "WEBAPPNAME");
        webappName = TestMode == Tests.Helpers.TestMode.Playback ? "Sanitized-webapp" : webappName;
        var resourceGroupName = RegisterOrRetrieveVariable("resourceGroupName", Settings.ResourceGroupName);

        var result = await CallToolAsync(
            "appservice_webapp_diagnostic_get-category",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", resourceGroupName },
                { "app", webappName }
            });

        var getResult = JsonSerializer.Deserialize(result.Value, AppServiceJsonContext.Default.WebappDiagnosticCategoryGetResult);
        Assert.NotNull(getResult);
        Assert.NotEmpty(getResult.WebappDiagnosticCategories);

        var categoryName = getResult.WebappDiagnosticCategories[0].Name;

        result = await CallToolAsync(
            "appservice_webapp_diagnostic_get-category",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", resourceGroupName },
                { "app", webappName },
                { "diagnostic-category", categoryName }
            });
        
        var categoryGetResult = JsonSerializer.Deserialize(result.Value, AppServiceJsonContext.Default.WebappDiagnosticCategoryGetResult);
        Assert.NotNull(categoryGetResult);
        Assert.Single(categoryGetResult.WebappDiagnosticCategories);
        Assert.Equal(categoryName, categoryGetResult.WebappDiagnosticCategories[0].Name);
    }
}
