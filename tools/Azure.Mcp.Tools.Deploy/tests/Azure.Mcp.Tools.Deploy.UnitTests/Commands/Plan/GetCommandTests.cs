// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.Deploy.Commands;
using Azure.Mcp.Tools.Deploy.Commands.Plan;
using Microsoft.Mcp.Tests.Client;
using Xunit;

namespace Azure.Mcp.Tools.Deploy.UnitTests.Commands.Plan;


public class GetCommandTests : CommandUnitTestsBase<GetCommand, object>
{
    [Fact]
    public async Task GetPlan_Should_Return_Expected_Result()
    {
        // arrange & act
        var result = await ExecuteCommandAsync(
            "--workspace-folder", "C:/",
            "--project-name", "django",
            "--target-app-service", "ContainerApp",
            "--provisioning-tool", "AZD",
            "--iac-options", "bicep");

        // assert
        var plan = ValidateAndDeserializeResponse(result, DeployJsonContext.Default.GetCommandResult);
        Assert.Contains("# Azure Deployment Plan for django Project", plan.Plan);
        Assert.Contains("Azure Container Apps", plan.Plan);
    }

    [Fact]
    public async Task Should_get_plan_with_default_iac_options()
    {
        // arrange & act
        var result = await ExecuteCommandAsync(
            "--workspace-folder", "C:/test",
            "--project-name", "myapp",
            "--target-app-service", "WebApp",
            "--provisioning-tool", "azd");

        // assert
        var plan = ValidateAndDeserializeResponse(result, DeployJsonContext.Default.GetCommandResult);
        Assert.Contains("# Azure Deployment Plan for myapp Project", plan.Plan);
        Assert.Contains("Azure Web App Service", plan.Plan);
    }

    [Fact]
    public async Task Should_get_plan_for_kubernetes()
    {
        // arrange & act
        var result = await ExecuteCommandAsync(
            "--workspace-folder", "C:/k8s-project",
            "--project-name", "k8s-app",
            "--target-app-service", "AKS",
            "--provisioning-tool", "azcli");

        // assert
        var plan = ValidateAndDeserializeResponse(result, DeployJsonContext.Default.GetCommandResult);
        Assert.Contains("# Azure Deployment Plan for k8s-app Project", plan.Plan);
        Assert.Contains("Azure Kubernetes Service", plan.Plan);
        Assert.Contains("Provision Azure Infrastructure with Azure CLI", plan.Plan);
        Assert.Contains("terraform", plan.Plan); // Default IaC option for aks
        Assert.Contains("Azure Kubernetes Service Deployment", plan.Plan);
    }

    [Fact]
    public async Task Should_get_plan_with_default_target_service()
    {
        // arrange & act
        var result = await ExecuteCommandAsync(
            "--workspace-folder", "C:/",
            "--project-name", "default-app",
            "--target-app-service", "unknown-service", // This should default to Container Apps
            "--provisioning-tool", "AZD");

        // assert
        var plan = ValidateAndDeserializeResponse(result, DeployJsonContext.Default.GetCommandResult);
        Assert.Contains("# Azure Deployment Plan for default-app Project", plan.Plan);
        Assert.Contains("Azure Container Apps", plan.Plan); // Should default to Container Apps
    }

    [Fact]
    public async Task Should_get_deploy_only_plan()
    {
        var result = await ExecuteCommandAsync(
            "--workspace-folder", "C:/",
            "--project-name", "default-app",
            "--target-app-service", "ContainerApp",
            "--provisioning-tool", "AzCli",
            "--deploy-option", "deploy-only",
            "--source-type", "from-azure",
            "--resource-group", "DefaultRG");

        // assert
        var plan = ValidateAndDeserializeResponse(result, DeployJsonContext.Default.GetCommandResult);
        Assert.Contains("# Azure Deployment Plan for default-app Project", plan.Plan);
        Assert.Contains("Azure Container Apps", plan.Plan); // Should default to Container Apps
        Assert.Contains("Containerization", plan.Plan);
        Assert.Contains("Check Azure resources existence", plan.Plan);
        Assert.Contains("Azure Container Registry", plan.Plan);
        Assert.Contains("**Existing Azure Resources**", plan.Plan);
    }
}
