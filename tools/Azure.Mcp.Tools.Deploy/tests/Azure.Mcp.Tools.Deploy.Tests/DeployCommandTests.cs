// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Tests.Client;
using Microsoft.Mcp.Tests.Client.Helpers;
using Xunit;

namespace Azure.Mcp.Tools.Deploy.Tests;

public class DeployCommandTests(ITestOutputHelper output, TestProxyFixture fixture, LiveServerFixture liveServerFixture)
    : RecordedCommandTestsBase(output, fixture, liveServerFixture)
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
                { "target-app-service", "ContainerApp" },
                { "provisioning-tool", "AZD" },
                { "iac-options", "bicep" }
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
                { "deployment-tool", "azd" },
                { "iac-type", "bicep" },
                { "resource-types", "appservice, azurestorage" }
            });

        Assert.Contains("Deployment Tool azd rules", result ?? string.Empty, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Should_get_infrastructure_rules_for_terraform()
    {
        // act
        var result = await CallToolMessageAsync(
            "deploy_iac_rules_get",
            new()
            {
                { "deployment-tool", "azd" },
                { "iac-type", "terraform" },
                { "resource-types", "containerapp, azurecosmosdb" }
            });

        // assert
        Assert.Contains("IaC Type: terraform rules", result ?? string.Empty, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Should_generate_pipeline()
    {
        // act
        var result = await CallToolMessageAsync(
            "deploy_pipeline_guidance_get",
            new()
            {
                { "subscription", _subscriptionId },
                { "is-azd-project", true }
            });

        // assert
        Assert.Contains("Use 'azd deploy --no-prompt' to skip provisioning in CD pipeline.", result);
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
                { "is-azd-project", false },
                { "pipeline-platform", "github-actions" },
                { "deploy-option", "deploy-only" }
            });

        // assert
        Assert.Contains("When user confirms that Azure resources are ready for deployment, you need to know at least two things", result ?? string.Empty);
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
        var result = await CallToolAsync(
            "deploy",
            command,
            parameters,
            resultProcessor: elem => elem.TryGetProperty("message", out var messageProp) ? messageProp : null);

        return result?.GetString() ?? null;
    }
}
