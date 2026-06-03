// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.AppService.Commands;
using Microsoft.Mcp.Tests.Client;
using Microsoft.Mcp.Tests.Client.Helpers;
using Microsoft.Mcp.Tests.Helpers;
using Xunit;

namespace Azure.Mcp.Tools.AppService.Tests.Webapp.Settings;

[Trait("Command", "AppSettingsUpdateCommand")]
public class AppSettingsUpdateCommandLiveTests(ITestOutputHelper output, TestProxyFixture fixture, LiveServerFixture liveServerFixture)
    : BaseAppServiceCommandLiveTests(output, fixture, liveServerFixture)
{
    [Fact]
    public async Task ExecuteAsync_AddSetting_AddingNewSettingWorks()
    {
        var webappName = RegisterOrRetrieveDeploymentOutputVariable("webappName", "WEBAPPNAME");
        webappName = TestMode == TestMode.Playback ? "Sanitized-webapp" : webappName;
        var resourceGroupName = RegisterOrRetrieveVariable("resourceGroupName", Settings.ResourceGroupName);
        var settingName = RegisterOrRetrieveVariable("settingName", RandomString());

        var result = await CallToolAsync(
            "appservice_webapp_settings_update-appsettings",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", resourceGroupName },
                { "app", webappName },
                { "setting-name", settingName },
                { "setting-value", "SomeValue" },
                { "setting-update-type", "add" }
            });

        var updateResult = DeserializeResult(result, AppServiceJsonContext.Default.AppSettingsUpdateResult);
        Assert.NotEmpty(updateResult.UpdateStatus);
        Assert.Contains($"Application setting '{settingName}' added", updateResult.UpdateStatus);
    }

    [Fact]
    public async Task ExecuteAsync_AddSettings_AddingExistingSettingDoesNotWork()
    {
        var webappName = RegisterOrRetrieveDeploymentOutputVariable("webappName", "WEBAPPNAME");
        webappName = TestMode == TestMode.Playback ? "Sanitized-webapp" : webappName;
        var resourceGroupName = RegisterOrRetrieveVariable("resourceGroupName", Settings.ResourceGroupName);
        var settingName = RegisterOrRetrieveVariable("settingName", RandomString());

        var result = await CallToolAsync(
            "appservice_webapp_settings_update-appsettings",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", resourceGroupName },
                { "app", webappName },
                { "setting-name", settingName },
                { "setting-value", "SomeValue" },
                { "setting-update-type", "add" }
            });

        var updateResult = DeserializeResult(result, AppServiceJsonContext.Default.AppSettingsUpdateResult);
        Assert.NotEmpty(updateResult.UpdateStatus);
        Assert.Contains($"Application setting '{settingName}' added", updateResult.UpdateStatus);

        result = await CallToolAsync(
            "appservice_webapp_settings_update-appsettings",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", resourceGroupName },
                { "app", webappName },
                { "setting-name", settingName },
                { "setting-value", "SomeValue" },
                { "setting-update-type", "add" }
            });

        updateResult = DeserializeResult(result, AppServiceJsonContext.Default.AppSettingsUpdateResult);
        Assert.NotEmpty(updateResult.UpdateStatus);
        Assert.Contains($"Failed to add application setting '{settingName}'", updateResult.UpdateStatus);
    }

    [Fact]
    public async Task ExecuteAsync_SetSettings_SettingAlwaysUpdates()
    {
        var webappName = RegisterOrRetrieveDeploymentOutputVariable("webappName", "WEBAPPNAME");
        webappName = TestMode == TestMode.Playback ? "Sanitized-webapp" : webappName;
        var resourceGroupName = RegisterOrRetrieveVariable("resourceGroupName", Settings.ResourceGroupName);
        var settingName = RegisterOrRetrieveVariable("settingName", RandomString());

        var result = await CallToolAsync(
            "appservice_webapp_settings_update-appsettings",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", resourceGroupName },
                { "app", webappName },
                { "setting-name", settingName },
                { "setting-value", "SomeValue" },
                { "setting-update-type", "set" }
            });

        var updateResult = DeserializeResult(result, AppServiceJsonContext.Default.AppSettingsUpdateResult);
        Assert.NotEmpty(updateResult.UpdateStatus);
        Assert.Contains($"Application setting '{settingName}' set", updateResult.UpdateStatus);

        result = await CallToolAsync(
            "appservice_webapp_settings_update-appsettings",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", resourceGroupName },
                { "app", webappName },
                { "setting-name", settingName },
                { "setting-value", "SomeValue" },
                { "setting-update-type", "set" }
            });

        updateResult = DeserializeResult(result, AppServiceJsonContext.Default.AppSettingsUpdateResult);
        Assert.NotEmpty(updateResult.UpdateStatus);
        Assert.Contains($"Application setting '{settingName}' set", updateResult.UpdateStatus);
    }

    [Fact]
    public async Task ExecuteAsync_DeleteSettings_DeletingAnExistingSettingWorks()
    {
        var webappName = RegisterOrRetrieveDeploymentOutputVariable("webappName", "WEBAPPNAME");
        webappName = TestMode == TestMode.Playback ? "Sanitized-webapp" : webappName;
        var resourceGroupName = RegisterOrRetrieveVariable("resourceGroupName", Settings.ResourceGroupName);
        var settingName = RegisterOrRetrieveVariable("settingName", RandomString());

        var result = await CallToolAsync(
            "appservice_webapp_settings_update-appsettings",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", resourceGroupName },
                { "app", webappName },
                { "setting-name", settingName },
                { "setting-value", "SomeValue" },
                { "setting-update-type", "set" }
            });

        var updateResult = DeserializeResult(result, AppServiceJsonContext.Default.AppSettingsUpdateResult);
        Assert.NotEmpty(updateResult.UpdateStatus);
        Assert.Contains($"Application setting '{settingName}' set", updateResult.UpdateStatus);

        result = await CallToolAsync(
            "appservice_webapp_settings_update-appsettings",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", resourceGroupName },
                { "app", webappName },
                { "setting-name", settingName },
                { "setting-update-type", "delete" }
            });

        updateResult = DeserializeResult(result, AppServiceJsonContext.Default.AppSettingsUpdateResult);
        Assert.NotEmpty(updateResult.UpdateStatus);
        Assert.Contains($"Application setting '{settingName}' deleted", updateResult.UpdateStatus);
    }

    [Fact]
    public async Task ExecuteAsync_DeleteSettings_DeletingDoesNotWorkOnNonExistentSetting()
    {
        var webappName = RegisterOrRetrieveDeploymentOutputVariable("webappName", "WEBAPPNAME");
        webappName = TestMode == TestMode.Playback ? "Sanitized-webapp" : webappName;
        var resourceGroupName = RegisterOrRetrieveVariable("resourceGroupName", Settings.ResourceGroupName);
        var settingName = RegisterOrRetrieveVariable("settingName", RandomString());

        var result = await CallToolAsync(
            "appservice_webapp_settings_update-appsettings",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", resourceGroupName },
                { "app", webappName },
                { "setting-name", settingName },
                { "setting-update-type", "delete" }
            });

        var updateResult = DeserializeResult(result, AppServiceJsonContext.Default.AppSettingsUpdateResult);
        Assert.NotEmpty(updateResult.UpdateStatus);
        Assert.Contains($"Application setting '{settingName}' doesn't exist", updateResult.UpdateStatus);
    }

    private static readonly char[] alphabet = ['a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z'];
    private static string RandomString() => Random.Shared.GetString(alphabet, 24);
}
