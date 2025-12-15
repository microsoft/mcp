// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Tests;
using Azure.Mcp.Tests.Client;
using Azure.Mcp.Tests.Client.Helpers;
using Azure.Mcp.Tests.Generated.Models;
using Xunit;

namespace Azure.Mcp.Tools.SignalR.LiveTests;

public sealed class SignalRCommandTests(ITestOutputHelper output, TestProxyFixture fixture) : RecordedCommandTestsBase(output, fixture)
{
    private const string SanitizedValue = "Sanitized";

    public override List<UriRegexSanitizer> UriRegexSanitizers { get; } = new List<UriRegexSanitizer>
     {
         new(new UriRegexSanitizerBody
         {
             Regex = "resource[Gg]roups/([^?\\/]+)",
             Value = SanitizedValue,
             GroupForReplace = "1"
         }),
         new(new UriRegexSanitizerBody
         {
             Regex = "signalR/([^?\\/]+)",
             Value = SanitizedValue,
             GroupForReplace = "1"
         })
      };

    public override List<BodyKeySanitizer> BodyKeySanitizers { get; } = new List<BodyKeySanitizer>
    {
        new(new BodyKeySanitizerBody("$..displayName")
        {
            Value = SanitizedValue
        }),
    };

    /// <summary>
    /// AZSDK3493 = $..name
    /// </summary>
    public override List<string> DisabledDefaultSanitizers => base.DisabledDefaultSanitizers.Concat(new[] { "AZSDK3493" }).ToList();

    [Fact]
    public async Task Should_get_signalr_runtimes_by_subscription_id()
    {
        var result = await CallToolAsync(
            "signalr_runtime_get",
            new() { { "subscription", Settings.SubscriptionId } });

        var runtimes = result.AssertProperty("runtimes");
        Assert.Equal(JsonValueKind.Array, runtimes.ValueKind);
        foreach (var runtime in runtimes.EnumerateArray())
        {
            Assert.Equal(JsonValueKind.Object, runtime.ValueKind);

            // Verify required properties exist
            var nameProperty = runtime.AssertProperty("name");
            Assert.False(string.IsNullOrEmpty(nameProperty.GetString()));
            var kindProperty = runtime.AssertProperty("kind");
            Assert.Equal("SignalR", kindProperty.GetString(), ignoreCase: true);
        }
    }

    [Fact]
    public async Task Should_handle_empty_subscription_gracefully()
    {
        // Empty subscription should trigger validation failure (400) -> null results
        var result = await CallToolAsync(
            "signalr_runtime_get",
            new() { { "subscription", "" } });

        Assert.Null(result);
    }

    [Fact]
    public async Task Should_handle_invalid_subscription_gracefully()
    {
        // Invalid identifier should reach execution and return structured error details (HasValue)
        var result = await CallToolAsync(
            "signalr_runtime_get",
            new()
            {
                    { "subscription", "invalid-subscription" }
            });

        Assert.NotNull(result);
        var message = result.AssertProperty("message");
        Assert.Contains("invalid-subscription", message.GetString(), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Should_get_signalr_runtimes_by_subscription_name()
    {
        var result = await CallToolAsync(
            "signalr_runtime_get",
            new() { { "subscription", Settings.SubscriptionName } });

        var runtimes = result.AssertProperty("runtimes");
        Assert.Equal(JsonValueKind.Array, runtimes.ValueKind);
        // Note: Array might be empty if no SignalR runtimes exist in subscription
    }

    [Fact]
    public async Task Should_get_signalr_runtimes_by_subscription_name_with_tenant_id()
    {
        var result = await CallToolAsync(
            "signalr_runtime_get",
            new() { { "subscription", Settings.SubscriptionName }, { "tenant", Settings.TenantId } });

        var runtimes = result.AssertProperty("runtimes");
        Assert.Equal(JsonValueKind.Array, runtimes.ValueKind);
        // Note: Array might be empty if no SignalR runtimes exist in subscription
    }

    [Fact]
    public async Task Should_get_signalr_runtimes_by_subscription_name_with_tenant_name()
    {
        Assert.SkipWhen(Settings.IsServicePrincipal, TenantNameReason);

        var result = await CallToolAsync(
            "signalr_runtime_get",
            new() { { "subscription", Settings.SubscriptionName }, { "tenant", Settings.TenantName } });

        var runtimes = result.AssertProperty("runtimes");
        Assert.Equal(JsonValueKind.Array, runtimes.ValueKind);
        // Note: Array might be empty if no SignalR runtimes exist in subscription
    }

    [Fact]
    public async Task Should_get_signalr_runtimes_by_subscription_with_resource_group()
    {
        Assert.SkipWhen(Settings.IsServicePrincipal, TenantNameReason);

        var result = await CallToolAsync(
            "signalr_runtime_get",
            new() { { "subscription", Settings.SubscriptionName }, { "resource-group", Settings.ResourceGroupName } });

        var runtimes = result.AssertProperty("runtimes");
        Assert.Equal(JsonValueKind.Array, runtimes.ValueKind);
        // Note: Array might be empty if no SignalR runtimes exist in subscription
    }

    [Fact]
    public async Task Should_get_signalr_runtime_detail()
    {
        var capturedRuntimeName = Settings.ResourceBaseName;

        var getResult = await CallToolAsync(
            "signalr_runtime_get",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "signalr", capturedRuntimeName }
            });

        var runtimes = getResult.AssertProperty("runtimes");
        var runtime = runtimes[0];
        Assert.Equal(JsonValueKind.Object, runtime.ValueKind);

        // Verify essential properties exist
        var nameProperty = runtime.AssertProperty("name");
        Assert.Equal(capturedRuntimeName, nameProperty.GetString());

        var kindProperty = runtime.AssertProperty("kind");
        Assert.Equal("SignalR", kindProperty.GetString(), ignoreCase: true);
    }
}
