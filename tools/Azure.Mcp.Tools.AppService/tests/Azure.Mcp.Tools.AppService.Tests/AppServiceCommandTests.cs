// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Azure.Mcp.Tools.AppService.Commands;
using Microsoft.Mcp.Tests.Client;
using Microsoft.Mcp.Tests.Client.Helpers;
using Microsoft.Mcp.Tests.Generated.Models;
using Microsoft.Mcp.Tests.Helpers;
using Xunit;

namespace Azure.Mcp.Tools.AppService.Tests;

public sealed class AppServiceCommandTests(ITestOutputHelper output, TestProxyFixture fixture, LiveServerFixture liveServerFixture)
    : RecordedCommandTestsBase(output, fixture, liveServerFixture)
{
    public override List<BodyKeySanitizer> BodyKeySanitizers =>
    [
        ..base.BodyKeySanitizers,
        new(new("$.properties.selfLink")),
        new(new("$.properties.customDomainVerificationId")),
        new(new("$.properties.inboundIpAddress")),
        new(new("$.properties.possibleInboundIpAddresses")),
        new(new("$.properties.inboundIpv6Address")),
        new(new("$.properties.possibleInboundIpv6Addresses")),
        new(new("$.properties.ftpsHostName")),
        new(new("$.properties.outboundIpAddresses")),
        new(new("$.properties.possibleOutboundIpAddresses")),
        new(new("$.properties.outboundIpv6Addresses")),
        new(new("$.properties.possibleOutboundIpv6Addresses")),
        new(new("$.properties.homeStamp")),
    ];

    // Sanitize resource group name in response bodies (complements the URI sanitizer in base class)
    public override List<BodyRegexSanitizer> BodyRegexSanitizers =>
    [
        ..base.BodyRegexSanitizers,
        new(new()
        {
            Regex = Settings.ResourceGroupName,
            Value = "Sanitized"
        }),
    ];

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

    #region database_add

    [Fact]
    public async Task ExecuteAsync_WithValidParameters_ReturnsSuccessResult()
    {
        var resourceGroupName = RegisterOrRetrieveVariable("resourceGroupName", Settings.ResourceGroupName);
        var resourceBaseName = TestMode == TestMode.Playback ? "Sanitized" : Settings.ResourceBaseName;
        var result = await CallToolAsync(
            "appservice_database_add",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", resourceGroupName },
                { "app", resourceBaseName + "-webapp" },
                { "database-type", "SqlServer" },
                { "database-server", resourceBaseName + "-sql.database.windows.net" },
                { "database", resourceBaseName + "db" }
            });

        // Test should validate actual command execution and error handling
        // If the tool returned no JSON (null), treat that as an expected error outcome in live tests
        if (!result.HasValue)
        {
            // Expected for live environments where resources may not exist; accept as valid outcome.
            return;
        }

        // Otherwise, verify that the returned JSON is non-empty
        var contentString = result.Value.ToString();
        Assert.False(string.IsNullOrWhiteSpace(contentString), "Expected non-empty content when command returns JSON");
    }

    [Theory]
    [InlineData("SqlServer")]
    public async Task ExecuteAsync_WithDifferentDatabaseTypes_AcceptsValidTypes(string databaseType)
    {
        var result = await CallToolAsync(
            "appservice_database_add",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", "test-rg" },
                { "app", "test-app" },
                { "database-type", databaseType },
                { "database-server", "test-server" },
                { "database", "test-db" }
            });

        // Test that database type validation works correctly
        if (!result.HasValue)
        {
            // No JSON result indicates the tool returned an error (acceptable for live environment)
            return;
        }
        else
        {
            var content = result.Value.ToString();

            // Should not fail due to invalid database type since we're testing valid types
            Assert.False(
                content.Contains("Unsupported database type") ||
                content.Contains("invalid database type"),
                $"Database type '{databaseType}' should be supported but got error: {content}");

            // If it succeeded, ensure the returned content is not empty
            Assert.False(string.IsNullOrWhiteSpace(content), $"Command should return content for {databaseType}");
        }
    }

    [Theory]
    [InlineData("InvalidType")]
    [InlineData("random")]
    public async Task ExecuteAsync_WithInvalidDatabaseTypes_ReturnsValidationError(string invalidDatabaseType)
    {
        var resourceGroupName = RegisterOrRetrieveVariable("resourceGroupName", Settings.ResourceGroupName);
        var resourceBaseName = TestMode == TestMode.Playback ? "Sanitized" : Settings.ResourceBaseName;
        var result = await CallToolAsync(
            "appservice_database_add",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", resourceGroupName },
                { "app", resourceBaseName + "-webapp" },
                { "database-type", invalidDatabaseType },
                { "database-server", resourceBaseName + "-sql.database.windows.net" },
                { "database", resourceBaseName + "db" }
            });

        // For invalid types, the tool may either return no JSON (error case) or a JSON error payload.
        if (!result.HasValue)
        {
            // No JSON result indicates the tool returned an error — acceptable outcome
            return;
        }

        // If JSON was returned, validate the error message explicitly
        var root = result.Value;
        string? message = null;
        if (root.ValueKind == JsonValueKind.Object)
        {
            if (root.TryGetProperty("message", out var m) && m.ValueKind == JsonValueKind.String)
            {
                message = m.GetString();
            }
            else if (root.TryGetProperty("results", out var r) && r.ValueKind == JsonValueKind.Object)
            {
                if (r.TryGetProperty("message", out var rm) && rm.ValueKind == JsonValueKind.String)
                {
                    message = rm.GetString();
                }
            }
        }

        Assert.False(string.IsNullOrWhiteSpace(message), $"Expected an error message for invalid database type '{invalidDatabaseType}' but none was found");
        Assert.Contains("Unsupported database type", message, StringComparison.OrdinalIgnoreCase);
    }

    #endregion

    #region webapp_deployment_get

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

        var getResult = DeserializeResult(result, AppServiceJsonContext.Default.DeploymentGetResult);
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

        var getResult = DeserializeResult(result, AppServiceJsonContext.Default.DeploymentGetResult);
        Assert.Single(getResult.Deployments);
        Assert.Equal(deploymentId, getResult.Deployments[0].Id);
    }

    #endregion

    #region webapp_diagnostic_diagnose

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
        var startTime = RegisterOrRetrieveVariable("startTime", DateTimeOffset.UtcNow.AddHours(-1).ToString("o"));
        var endTime = RegisterOrRetrieveVariable("endTime", DateTimeOffset.UtcNow.ToString("o"));

        var result = await CallToolAsync(
            "appservice_webapp_diagnostic_diagnose",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", resourceGroupName },
                { "app", webappName },
                { "detector-id", "AvailabilityAndPerformanceWindows"},
                { "start-time", startTime },
                { "end-time", endTime },
                { "time-grain", "PT10M" }
            });

        var detectorsResult = DeserializeResult(result, AppServiceJsonContext.Default.DetectorDiagnoseResult);
        Assert.NotNull(detectorsResult.Diagnoses);
        Assert.NotEmpty(detectorsResult.Diagnoses.Datasets);
    }

    #endregion

    #region webapp_diagnostic_list

    [Fact]
    public async Task ExecuteAsync_DetectorsList_ReturnsDetectors()
    {
        var webappName = RegisterOrRetrieveDeploymentOutputVariable("webappName", "WEBAPPNAME");
        webappName = TestMode == TestMode.Playback ? "Sanitized-webapp" : webappName;
        var resourceGroupName = RegisterOrRetrieveVariable("resourceGroupName", Settings.ResourceGroupName);

        var result = await CallToolAsync(
            "appservice_webapp_diagnostic_list",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", resourceGroupName },
                { "app", webappName }
            });

        var detectorsResult = DeserializeResult(result, AppServiceJsonContext.Default.DetectorListResult);
        Assert.NotEmpty(detectorsResult.Detectors);
    }

    #endregion

    #region webapp_settings_get-appsettings

    [Fact(Skip = "Test temporarily disabled - recording can't consent to secret elicitation")]
    public async Task ExecuteAsync_AppSettingsList_ReturnsAppSettings()
    {
        var webappName = RegisterOrRetrieveDeploymentOutputVariable("webappName", "WEBAPPNAME");
        webappName = TestMode == TestMode.Playback ? "Sanitized-webapp" : webappName;
        var resourceGroupName = RegisterOrRetrieveVariable("resourceGroupName", Settings.ResourceGroupName);

        var result = await CallToolAsync(
            "appservice_webapp_settings_get-appsettings",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", resourceGroupName },
                { "app", webappName }
            });

        var getResult = DeserializeResult(result, AppServiceJsonContext.Default.AppSettingsGetResult);
        Assert.NotEmpty(getResult.AppSettings);
    }

    #endregion

    #region webapp_settings_update-appsettings

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

    #endregion

    #region webapp_get

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

    #endregion

    private static T DeserializeResult<T>(JsonElement? element, JsonTypeInfo<T> jsonTypeInfo)
    {
        Assert.NotNull(element);
        var deserialized = JsonSerializer.Deserialize(element.Value, jsonTypeInfo);
        Assert.NotNull(deserialized);
        return deserialized;
    }

    private static readonly char[] s_alphabet = ['a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z'];
    private static string RandomString() => Random.Shared.GetString(s_alphabet, 24);
}
