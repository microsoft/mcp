// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Nodes;
using Azure.Mcp.Tests.Client;
using Azure.Mcp.Tests.Client.Helpers;
using ModelContextProtocol.Client;
using Xunit;

namespace Azure.Mcp.Tools.Deploy.LiveTests;

public class DeployCommandTests(ITestOutputHelper output) : CommandTestsBase(output)
{
    private string _subscriptionId = default!;

    public override async ValueTask InitializeAsync()
    {
        await base.InitializeAsync();
        _subscriptionId = Settings.SubscriptionId;
    }

    [Fact]
    public async Task Should_get_plan()
    {
        // act
        var result = await CallToolMessageAsync(
            "deploy_plan_get",
            new()
            {
                { "workspace-folder", "C:/" },
                { "project-name", "django" },
                { "target-app-service", "ContainerApp" }
            });
        // assert
        Assert.StartsWith("# Azure Deployment Plan for django Project", result);
    }

    [Fact]
    public async Task Should_get_infrastructure_code_rules()
    {
        // act
        var result = await CallToolMessageAsync(
            "deploy_iac_rules_get",
            new()
            {
                { "deployment-tool", "AzCli" },
                { "resource-types", "appservice, azurestorage" }
            });

        Assert.Contains("Deployment Tool AzCli rules", result ?? string.Empty, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Should_generate_pipeline_with_github_details()
    {
        // act
        var result = await CallToolMessageAsync(
            "deploy_pipeline_guidance_get",
            new()
            {
                { "subscription", _subscriptionId },
                { "organization-name", "test-org" },
                { "repository-name", "test-repo" },
                { "github-environment-name", "production" }
            });

        // assert
        Assert.StartsWith("Help the user to set up a CI/CD pipeline", result ?? string.Empty);
    }

    // skip as this test need local files
    // [Fact]
    // public async Task Should_get_azd_app_logs()
    // {
    //     // act
    //     var result = await CallToolMessageAsync(
    //         "deploy_app_logs_get",
    //         new()
    //         {
    //             { "subscription", _subscriptionId },
    //             { "workspace-folder", "C:/Users/" },
    //             { "azd-env-name", "dotnet-demo" },
    //             { "limit", 10 }
    //         });

    //     // assert
    //     Assert.StartsWith("App logs retrieved:", result);
    // }


    private async Task<string?> CallToolMessageAsync(string command, Dictionary<string, object?> parameters)
    {
        // Output will be streamed, so if we're not in debug mode, hold the debug output for logging in the failure case
        Action<string> writeOutput = Settings.DebugOutput
            ? s => Output.WriteLine(s)
            : s => FailureOutput.AppendLine(s);

        writeOutput($"request: {JsonSerializer.Serialize(new { command, parameters })}");

        var result = await Client.CallToolAsync(command, parameters);

        var content = McpTestUtilities.GetFirstText(result.Content);
        if (string.IsNullOrWhiteSpace(content))
        {
            Output.WriteLine($"response: {JsonSerializer.Serialize(result)}");
            throw new Exception("No JSON content found in the response.");
        }

        var root = JsonSerializer.Deserialize<JsonElement>(content!);
        if (root.ValueKind != JsonValueKind.Object)
        {
            Output.WriteLine($"response: {JsonSerializer.Serialize(result)}");
            throw new Exception("Invalid JSON response.");
        }

        // Remove the `args` property and log the content
        var trimmed = root.Deserialize<JsonObject>()!;
        trimmed.Remove("args");
        writeOutput($"response content: {trimmed.ToJsonString(new JsonSerializerOptions { WriteIndented = true })}");

        return root.TryGetProperty("message", out var property) ? property.GetString() : null;
    }

}
