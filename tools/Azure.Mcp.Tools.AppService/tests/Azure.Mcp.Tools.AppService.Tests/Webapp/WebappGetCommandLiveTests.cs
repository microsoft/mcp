// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.AppService.Commands;
using Microsoft.Mcp.Tests.Client;
using Microsoft.Mcp.Tests.Client.Helpers;
using Microsoft.Mcp.Tests.Helpers;
using Xunit;

namespace Azure.Mcp.Tools.AppService.Tests.Webapp;

[Trait("Command", "WebappGetCommand")]
public class WebappGetCommandLiveTests(ITestOutputHelper output, TestProxyFixture fixture, LiveServerFixture liveServerFixture)
    : BaseAppServiceCommandLiveTests(output, fixture, liveServerFixture)
{
    public override string[] Tools => ["appservice_webapp_get"];

    [Fact]
    public async Task ExecuteAsync_SubscriptionList_ReturnsExpectedWebApp()
    {
        var webappName = RegisterOrRetrieveDeploymentOutputVariable("webappName", "WEBAPPNAME");
        var expectedWebappName = TestMode == TestMode.Playback ? "Sanitized" : webappName;

        var result = await CallToolAsync(
            "appservice_webapp_get",
            new()
            {
                { "subscription", Settings.SubscriptionId }
            });

        var getResult = DeserializeResult(result, AppServiceJsonContext.Default.WebappGetResult);
        Assert.NotEmpty(getResult.Webapps);
        Assert.True(getResult.Webapps.Any(detail => detail.Name == expectedWebappName), $"Expected to find web app with name '{expectedWebappName}' in the results.");
    }

    [Fact]
    public async Task ExecuteAsync_ResourceGroupList_ReturnsExpectedWebApp()
    {
        var webappName = RegisterOrRetrieveDeploymentOutputVariable("webappName", "WEBAPPNAME");
        var expectedWebappName = TestMode == TestMode.Playback ? "Sanitized" : webappName;
        var resourceGroupName = RegisterOrRetrieveVariable("resourceGroupName", Settings.ResourceGroupName);

        var result = await CallToolAsync(
            "appservice_webapp_get",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", resourceGroupName }
            });

        var getResult = DeserializeResult(result, AppServiceJsonContext.Default.WebappGetResult);
        Assert.NotEmpty(getResult.Webapps);
        Assert.True(getResult.Webapps.Any(detail => detail.Name == expectedWebappName), $"Expected to find web app with name '{expectedWebappName}' in the results.");
    }

    [Fact]
    public async Task ExecuteAsync_WebAppGet_ReturnsExpectedWebApp()
    {
        var webappName = RegisterOrRetrieveDeploymentOutputVariable("webappName", "WEBAPPNAME");
        var expectedWebappName = TestMode == TestMode.Playback ? "Sanitized" : webappName;
        webappName = TestMode == TestMode.Playback ? "Sanitized-webapp" : webappName;
        var resourceGroupName = RegisterOrRetrieveVariable("resourceGroupName", Settings.ResourceGroupName);

        var result = await CallToolAsync(
            "appservice_webapp_get",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", resourceGroupName },
                { "app", webappName }
            });

        var getResult = DeserializeResult(result, AppServiceJsonContext.Default.WebappGetResult);
        Assert.Single(getResult.Webapps);
        Assert.True(getResult.Webapps.All(detail => detail.Name == expectedWebappName), $"Expected to find a single web app with name '{expectedWebappName}' in the results.");
    }
}
