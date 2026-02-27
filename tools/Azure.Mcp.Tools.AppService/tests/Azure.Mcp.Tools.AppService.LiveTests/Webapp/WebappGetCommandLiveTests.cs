// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Tests.Client;
using Azure.Mcp.Tests.Client.Helpers;
using Azure.Mcp.Tools.AppService.Commands;
using Xunit;

namespace Azure.Mcp.Tools.AppService.LiveTests.Webapp;

[Trait("Command", "WebappGetCommand")]
public class WebappGetCommandLiveTests(ITestOutputHelper output, TestProxyFixture fixture, LiveServerFixture liveServerFixture)
    : BaseAppServiceCommandLiveTests(output, fixture, liveServerFixture)
{

    [Fact]
    public async Task ExecuteAsync_SubscriptionList_ReturnsExpectedWebApp()
    {
        var webappName = RegisterOrRetrieveDeploymentOutputVariable("webappName", "WEBAPPNAME");
        var expectedWebappName = TestMode == Tests.Helpers.TestMode.Playback ? "Sanitized" : webappName;

        var result = await CallToolAsync(
            "appservice_webapp_get",
            new()
            {
                { "subscription", Settings.SubscriptionId }
            });

        var getResult = JsonSerializer.Deserialize(result.Value, AppServiceJsonContext.Default.WebappGetResult);
        Assert.NotNull(getResult);
        Assert.NotEmpty(getResult.Webapps);
        Assert.True(getResult.Webapps.Any(detail => detail.Name == expectedWebappName), $"Expected to find web app with name '{expectedWebappName}' in the results.");
    }

    [Fact]
    public async Task ExecuteAsync_ResourceGroupList_ReturnsExpectedWebApp()
    {
        var webappName = RegisterOrRetrieveDeploymentOutputVariable("webappName", "WEBAPPNAME");
        var expectedWebappName = TestMode == Tests.Helpers.TestMode.Playback ? "Sanitized" : webappName;
        var resourceGroupName = RegisterOrRetrieveVariable("resourceGroupName", Settings.ResourceGroupName);

        var result = await CallToolAsync(
            "appservice_webapp_get",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", resourceGroupName }
            });

        var getResult = JsonSerializer.Deserialize(result.Value, AppServiceJsonContext.Default.WebappGetResult);
        Assert.NotNull(getResult);
        Assert.NotEmpty(getResult.Webapps);
        Assert.True(getResult.Webapps.Any(detail => detail.Name == expectedWebappName), $"Expected to find web app with name '{expectedWebappName}' in the results.");
    }

    [Fact]
    public async Task ExecuteAsync_WebAppGet_ReturnsExpectedWebApp()
    {
        var webappName = RegisterOrRetrieveDeploymentOutputVariable("webappName", "WEBAPPNAME");
        var expectedWebappName = TestMode == Tests.Helpers.TestMode.Playback ? "Sanitized" : webappName;
        webappName = TestMode == Tests.Helpers.TestMode.Playback ? "Sanitized-webapp" : webappName;
        var resourceGroupName = RegisterOrRetrieveVariable("resourceGroupName", Settings.ResourceGroupName);

        var result = await CallToolAsync(
            "appservice_webapp_get",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", resourceGroupName },
                { "app", webappName }
            });

        var getResult = JsonSerializer.Deserialize(result.Value, AppServiceJsonContext.Default.WebappGetResult);
        Assert.NotNull(getResult);
        Assert.Single(getResult.Webapps);
        Assert.True(getResult.Webapps.All(detail => detail.Name == expectedWebappName), $"Expected to find a single web app with name '{expectedWebappName}' in the results.");
    }
}
