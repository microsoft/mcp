// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Tests.Client;
using Azure.Mcp.Tests.Client.Helpers;
using Azure.Mcp.Tools.AppService.Commands;
using Xunit;

namespace Azure.Mcp.Tools.AppService.LiveTests.Webapp.Settings;

[Trait("Command", "AppSettingsGetCommand")]
public class AppSettingsGetCommandLiveTests(ITestOutputHelper output, TestProxyFixture fixture, LiveServerFixture liveServerFixture)
    : BaseAppServiceCommandLiveTests(output, fixture, liveServerFixture)
{
    [Fact(Skip = "Test temporarily disabled - recording can't consent to secret elicitation")]
    public async Task ExecuteAsync_AppSettingsList_ReturnsAppSettings()
    {
        var webappName = RegisterOrRetrieveDeploymentOutputVariable("webappName", "WEBAPPNAME");
        webappName = TestMode == Tests.Helpers.TestMode.Playback ? "Sanitized-webapp" : webappName;
        var resourceGroupName = RegisterOrRetrieveVariable("resourceGroupName", Settings.ResourceGroupName);

        var result = await CallToolAsync(
            "appservice_webapp_settings_get-appsettings",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", resourceGroupName },
                { "app", webappName }
            });

        var getResult = JsonSerializer.Deserialize(result.Value, AppServiceJsonContext.Default.AppSettingsGetResult);
        Assert.NotNull(getResult);
        Assert.NotEmpty(getResult.AppSettings);
    }
}
