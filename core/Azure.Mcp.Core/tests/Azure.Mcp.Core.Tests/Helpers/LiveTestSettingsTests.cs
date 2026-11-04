// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Microsoft.Mcp.Tests.Client.Helpers;
using Microsoft.Mcp.Tests.Helpers;
using Xunit;

namespace Azure.Mcp.Core.Tests.Helpers;

public class LiveTestSettingsTests
{
    [Fact]
    public void Clone_PreservesSettingsAndCopiesMutableDictionaries()
    {
        var settings = new LiveTestSettings
        {
            PrincipalName = "test-principal",
            IsServicePrincipal = true,
            TenantId = "test-tenant",
            TenantName = "test-tenant-name",
            SubscriptionId = "test-subscription",
            SubscriptionName = "test-subscription-name",
            ResourceGroupName = "test-group",
            ResourceBaseName = "test-resource",
            SettingsDirectory = "test-directory",
            TestPackage = "test-package",
            TestMode = TestMode.Record,
            DebugOutput = true,
            DeploymentOutputs = new() { ["resource"] = "original" },
            EnvironmentVariables = new() { ["setting"] = "original" }
        };

        var clone = settings.Clone();

        Assert.Equivalent(settings, clone);
        clone.DeploymentOutputs["resource"] = "changed";
        clone.EnvironmentVariables["setting"] = "changed";
        Assert.Equal("original", settings.DeploymentOutputs["resource"]);
        Assert.Equal("original", settings.EnvironmentVariables["setting"]);
    }

    [Theory]
    [InlineData("Live", TestMode.Live)]
    [InlineData("Record", TestMode.Record)]
    [InlineData("Playback", TestMode.Playback)]
    [InlineData("playback", TestMode.Playback)]
    public void DeserializeTestMode_WithValidValue_ReturnsExpectedMode(string value, TestMode expected)
    {
        var settings = JsonSerializer.Deserialize<LiveTestSettings>($$"""{ "TestMode": "{{value}}" }""");

        Assert.NotNull(settings);
        Assert.Equal(expected, settings.TestMode);
    }

    [Fact]
    public void DeserializeTestMode_WithInvalidString_ThrowsActionableException()
    {
        var exception = Assert.Throws<JsonException>(
            () => JsonSerializer.Deserialize<LiveTestSettings>("""{ "TestMode": "Replay" }"""));

        Assert.Contains("Invalid TestMode 'Replay'. TestMode must be one of: Live, Record, Playback.", exception.Message);
    }

    [Fact]
    public void DeserializeTestMode_WithNonStringValue_ThrowsActionableException()
    {
        var exception = Assert.Throws<JsonException>(
            () => JsonSerializer.Deserialize<LiveTestSettings>("""{ "TestMode": 1 }"""));

        Assert.Contains("Invalid TestMode value. TestMode must be one of: Live, Record, Playback.", exception.Message);
    }
}
