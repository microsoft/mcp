// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Tests;
using Azure.Mcp.Tests.Client;
using Azure.Mcp.Tests.Client.Helpers;
using Xunit;

namespace Azure.Mcp.Core.LiveTests;

public class CommandTests(LiveTestFixture liveTestFixture, ITestOutputHelper output)
    : CommandTestsBase(liveTestFixture, output),
    IClassFixture<LiveTestFixture>
{
    [Fact]
    public async Task Should_list_groups_by_subscription()
    {
        var result = await CallToolAsync(
            "azmcp_group_list",
            new()
            {
                { "subscription", Settings.SubscriptionId }
            });

        var groupsArray = result.AssertProperty("groups");
        Assert.Equal(JsonValueKind.Array, groupsArray.ValueKind);
        Assert.NotEmpty(groupsArray.EnumerateArray());
    }

    [Fact]
    public async Task Should_get_general_code_generation_best_practices()
    {
        // Act
        JsonElement? result = await CallToolAsync("azmcp_get_bestpractices_get", new Dictionary<string, object?>
        {
            { "resource", "general" },
            { "action", "code-generation" }
        });

        Assert.True(result.HasValue, "Tool call did not return a value.");

        Assert.Equal(JsonValueKind.Array, result.Value.ValueKind);
        var entries = result.Value.EnumerateArray().ToList();
        Assert.NotEmpty(entries);

        // Combine all entries into a single normalized string for content assertion
        var combinedText = string.Join("\n", entries
            .Where(e => e.ValueKind == JsonValueKind.String)
            .Select(e => e.GetString())
            .Where(s => !string.IsNullOrWhiteSpace(s)));

        // Assert specific practices are mentioned based on azure-general-codegen-best-practices.txt content
        Assert.Contains("Managed Identity (Azure-hosted)", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("retry logic", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Generate secure, efficient, and maintainable Azure service code following these requirements:", combinedText, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Should_get_general_deployment_best_practices()
    {
        // Act
        JsonElement? result = await CallToolAsync("azmcp_get_bestpractices_get", new Dictionary<string, object?>
        {
            { "resource", "general" },
            { "action", "deployment" }
        });

        Assert.True(result.HasValue, "Tool call did not return a value.");

        Assert.Equal(JsonValueKind.Array, result.Value.ValueKind);
        var entries = result.Value.EnumerateArray().ToList();
        Assert.NotEmpty(entries);

        // Combine all entries into a single normalized string for content assertion
        var combinedText = string.Join("\n", entries
            .Where(e => e.ValueKind == JsonValueKind.String)
            .Select(e => e.GetString())
            .Where(s => !string.IsNullOrWhiteSpace(s)));

        // Assert specific practices are mentioned based on azure-general-deployment-best-practices.txt content
        Assert.Contains("Infrastructure as Code (IaC)", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("azd commands such as 'azd up'", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Bicep files under the 'infra/' folder", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("azure_recommend_service_config", combinedText, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Should_get_azure_functions_code_generation_best_practices()
    {
        // Act
        JsonElement? result = await CallToolAsync("azmcp_get_bestpractices_get", new Dictionary<string, object?>
        {
            { "resource", "azurefunctions" },
            { "action", "code-generation" }
        });

        Assert.True(result.HasValue, "Tool call did not return a value.");

        Assert.Equal(JsonValueKind.Array, result.Value.ValueKind);
        var entries = result.Value.EnumerateArray().ToList();
        Assert.NotEmpty(entries);

        // Combine all entries into a single normalized string for content assertion
        var combinedText = string.Join("\n", entries
            .Where(e => e.ValueKind == JsonValueKind.String)
            .Select(e => e.GetString())
            .Where(s => !string.IsNullOrWhiteSpace(s)));

        // Assert specific Azure Functions code generation practices are mentioned
        Assert.Contains("Use the latest programming models", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Azure Functions Core Tools", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("extension bundles", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("isolated process model", combinedText, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Should_get_azure_functions_deployment_best_practices()
    {
        // Act
        JsonElement? result = await CallToolAsync("azmcp_get_bestpractices_get", new Dictionary<string, object?>
        {
            { "resource", "azurefunctions" },
            { "action", "deployment" }
        });

        Assert.True(result.HasValue, "Tool call did not return a value.");

        Assert.Equal(JsonValueKind.Array, result.Value.ValueKind);
        var entries = result.Value.EnumerateArray().ToList();
        Assert.NotEmpty(entries);

        // Combine all entries into a single normalized string for content assertion
        var combinedText = string.Join("\n", entries
            .Where(e => e.ValueKind == JsonValueKind.String)
            .Select(e => e.GetString())
            .Where(s => !string.IsNullOrWhiteSpace(s)));

        // Assert specific Azure Functions deployment practices are mentioned
        Assert.Contains("Flex Consumption plan (FC1)", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Always use Linux OS for Python", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Function authentication", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Application Insights", combinedText, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Should_get_static_web_app_best_practices()
    {
        // Act
        JsonElement? result = await CallToolAsync("azmcp_get_bestpractices_get", new Dictionary<string, object?>
        {
            { "resource", "static-web-app" },
            { "action", "all" }
        });

        Assert.True(result.HasValue, "Tool call did not return a value.");

        Assert.Equal(JsonValueKind.Array, result.Value.ValueKind);
        var entries = result.Value.EnumerateArray().ToList();
        Assert.NotEmpty(entries);

        // Combine all entries into a single normalized string for content assertion
        var combinedText = string.Join("\n", entries
            .Where(e => e.ValueKind == JsonValueKind.String)
            .Select(e => e.GetString())
            .Where(s => !string.IsNullOrWhiteSpace(s)));

        // Assert specific Static Web Apps practices are mentioned
        Assert.Contains("Static Web Apps CLI", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("npx swa init --yes", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("npx swa build", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("npx swa deploy --env production", combinedText, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Should_get_general_all_best_practices()
    {
        // Act
        JsonElement? result = await CallToolAsync("azmcp_get_bestpractices_get", new Dictionary<string, object?>
        {
            { "resource", "general" },
            { "action", "all" }
        });

        Assert.True(result.HasValue, "Tool call did not return a value.");

        Assert.Equal(JsonValueKind.Array, result.Value.ValueKind);
        var entries = result.Value.EnumerateArray().ToList();
        Assert.NotEmpty(entries);

        // Combine all entries into a single normalized string for content assertion
        var combinedText = string.Join("\n", entries
            .Where(e => e.ValueKind == JsonValueKind.String)
            .Select(e => e.GetString())
            .Where(s => !string.IsNullOrWhiteSpace(s)));

        // Should contain content from both code-generation and deployment files
        Assert.Contains("Managed Identity (Azure-hosted)", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("retry logic", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Generate secure, efficient, and maintainable Azure service code following these requirements:", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Infrastructure as Code (IaC)", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("azd commands such as 'azd up'", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Bicep files under the 'infra/' folder", combinedText, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Should_get_azure_functions_all_best_practices()
    {
        // Act
        JsonElement? result = await CallToolAsync("azmcp_get_bestpractices_get", new Dictionary<string, object?>
        {
            { "resource", "azurefunctions" },
            { "action", "all" }
        });

        Assert.True(result.HasValue, "Tool call did not return a value.");

        Assert.Equal(JsonValueKind.Array, result.Value.ValueKind);
        var entries = result.Value.EnumerateArray().ToList();
        Assert.NotEmpty(entries);

        // Combine all entries into a single normalized string for content assertion
        var combinedText = string.Join("\n", entries
            .Where(e => e.ValueKind == JsonValueKind.String)
            .Select(e => e.GetString())
            .Where(s => !string.IsNullOrWhiteSpace(s)));

        // Should contain content from both code-generation and deployment files
        Assert.Contains("Use the latest programming models", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Azure Functions Core Tools", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("extension bundles", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("isolated process model", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Flex Consumption plan (FC1)", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Always use Linux OS for Python", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Function authentication", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Application Insights", combinedText, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Should_list_subscriptions()
    {
        var result = await CallToolAsync(
            "azmcp_subscription_list",
            new Dictionary<string, object?>());

        var subscriptionsArray = result.AssertProperty("subscriptions");
        Assert.Equal(JsonValueKind.Array, subscriptionsArray.ValueKind);
        Assert.NotEmpty(subscriptionsArray.EnumerateArray());
    }
}
