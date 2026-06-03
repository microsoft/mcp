// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.AppService.Commands;
using Microsoft.Mcp.Tests.Client;
using Microsoft.Mcp.Tests.Client.Helpers;
using Microsoft.Mcp.Tests.Helpers;
using Xunit;

namespace Azure.Mcp.Tools.AppService.Tests.Webapp.Diagnostic;

[Trait("Command", "DetectorDiagnoseCommand")]
public class DetectorDiagnoseCommandLiveTests(ITestOutputHelper output, TestProxyFixture fixture, LiveServerFixture liveServerFixture)
    : BaseAppServiceCommandLiveTests(output, fixture, liveServerFixture)
{
    [Fact]
    public async Task ExecuteAsync_DetectorsDiagnose_ReturnsDiagnostics()
    {
        var webappName = RegisterOrRetrieveDeploymentOutputVariable("webappName", "WEBAPPNAME");
        webappName = TestMode == TestMode.Playback ? "Sanitized-webapp" : webappName;
        var resourceGroupName = RegisterOrRetrieveVariable("resourceGroupName", Settings.ResourceGroupName);

        var result = await CallToolAsync(
            "appservice_webapp_diagnostic_diagnose",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", resourceGroupName },
                { "app", webappName },
                { "detector-id", "AvailabilityAndPerformanceWindows"}
            });

        var detectorsResult = DeserializeResult(result, AppServiceJsonContext.Default.DetectorDiagnoseResult);
        Assert.NotNull(detectorsResult.Diagnoses);
        Assert.NotEmpty(detectorsResult.Diagnoses.Datasets);
    }

    [Fact]
    public async Task ExecuteAsync_DetectorsDiagnoseWithOptionalParams_ReturnsDiagnostics()
    {
        var webappName = RegisterOrRetrieveDeploymentOutputVariable("webappName", "WEBAPPNAME");
        webappName = TestMode == TestMode.Playback ? "Sanitized-webapp" : webappName;
        var resourceGroupName = RegisterOrRetrieveVariable("resourceGroupName", Settings.ResourceGroupName);

        var result = await CallToolAsync(
            "appservice_webapp_diagnostic_diagnose",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", resourceGroupName },
                { "app", webappName },
                { "detector-id", "AvailabilityAndPerformanceWindows"},
                { "start-time", DateTimeOffset.UtcNow.AddHours(-1).ToString("o") },
                { "end-time", DateTimeOffset.UtcNow.ToString("o") },
                { "time-grain", "PT10M" }
            });

        var detectorsResult = DeserializeResult(result, AppServiceJsonContext.Default.DetectorDiagnoseResult);
        Assert.NotNull(detectorsResult.Diagnoses);
        Assert.NotEmpty(detectorsResult.Diagnoses.Datasets);
    }
}
