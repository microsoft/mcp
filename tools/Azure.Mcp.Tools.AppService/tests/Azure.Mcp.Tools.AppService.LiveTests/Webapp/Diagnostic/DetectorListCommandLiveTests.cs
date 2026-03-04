// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Tests.Client;
using Azure.Mcp.Tests.Client.Helpers;
using Azure.Mcp.Tools.AppService.Commands;
using Xunit;

namespace Azure.Mcp.Tools.AppService.LiveTests.Webapp.Diagnostic;

[Trait("Command", "DetectorListCommand")]
public class DetectorListCommandLiveTests(ITestOutputHelper output, TestProxyFixture fixture, LiveServerFixture<AzureLiveTestSettings> liveServerFixture)
    : BaseAppServiceCommandLiveTests(output, fixture, liveServerFixture)
{
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

        var detectorsResult = JsonSerializer.Deserialize(result.Value, AppServiceJsonContext.Default.DetectorListResult);
        Assert.NotNull(detectorsResult);
        Assert.NotEmpty(detectorsResult.Detectors);
    }
}
