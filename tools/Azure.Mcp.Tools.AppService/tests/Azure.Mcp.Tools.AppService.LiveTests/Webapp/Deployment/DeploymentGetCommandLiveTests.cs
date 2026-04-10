// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Tools.AppService.Commands;
using Microsoft.Mcp.Tests.Client;
using Microsoft.Mcp.Tests.Client.Helpers;
using Microsoft.Mcp.Tests.Generated.Models;
using Microsoft.Mcp.Tests.Helpers;
using Xunit;

namespace Azure.Mcp.Tools.AppService.LiveTests.Webapp.Deployment;

[Trait("Command", "DeploymentGetCommand")]
public class DeploymentGetCommandLiveTests(ITestOutputHelper output, TestProxyFixture fixture, LiveServerFixture liveServerFixture)
    : BaseAppServiceCommandLiveTests(output, fixture, liveServerFixture)
{
    public override CustomDefaultMatcher? TestMatcher
    {
        get
        {
            var matcher = base.TestMatcher ?? new CustomDefaultMatcher();
            matcher.IgnoredHeaders = string.IsNullOrEmpty(matcher.IgnoredHeaders) ? "Cookie" : $"{matcher.IgnoredHeaders},Cookie";
            matcher.ExcludedHeaders = string.IsNullOrEmpty(matcher.ExcludedHeaders) ? "Cookie" : $"{matcher.ExcludedHeaders},Cookie";
            return matcher;
        }
    }

    [Fact]
    public async Task ExecuteAsync_DeploymentList_ReturnsDeployments()
    {
        var webappName = RegisterOrRetrieveDeploymentOutputVariable("webappName", "WEBAPPNAME");
        webappName = TestMode == TestMode.Playback ? "Sanitized-webapp" : webappName;
        var resourceGroupName = RegisterOrRetrieveVariable("resourceGroupName", Settings.ResourceGroupName);
        var deploymentId = RegisterOrRetrieveDeploymentOutputVariable("deploymentId", "DEPLOYMENTID");

        var result = await CallToolAsync(
            "appservice_webapp_deployment_get",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", resourceGroupName },
                { "app", webappName }
            });

        var getResult = JsonSerializer.Deserialize(result.Value, AppServiceJsonContext.Default.DeploymentGetResult);
        Assert.NotNull(getResult);
        Assert.NotEmpty(getResult.Deployments);
        Assert.Contains(getResult.Deployments, d => d.Id == deploymentId);
    }

    [Fact]
    public async Task ExecuteAsync_DeploymentGet_ReturnsSpecificDeployment()
    {
        var webappName = RegisterOrRetrieveDeploymentOutputVariable("webappName", "WEBAPPNAME");
        webappName = TestMode == TestMode.Playback ? "Sanitized-webapp" : webappName;
        var resourceGroupName = RegisterOrRetrieveVariable("resourceGroupName", Settings.ResourceGroupName);
        var deploymentId = RegisterOrRetrieveDeploymentOutputVariable("deploymentId", "DEPLOYMENTID");

        var result = await CallToolAsync(
            "appservice_webapp_deployment_get",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", resourceGroupName },
                { "app", webappName },
                { "deployment-id", deploymentId }
            });

        var getResult = JsonSerializer.Deserialize(result.Value, AppServiceJsonContext.Default.DeploymentGetResult);
        Assert.NotNull(getResult);
        Assert.Single(getResult.Deployments);
        Assert.Equal(deploymentId, getResult.Deployments[0].Id);
    }
}
