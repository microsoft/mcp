// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
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
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.OK, result.Status);
        Assert.NotNull(result.Message);
        Assert.Contains("# Azure Deployment Plan for django Project", result.Message);
        Assert.Contains("Azure Container Apps", result.Message);
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
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.OK, result.Status);
        Assert.NotNull(result.Message);
        Assert.Contains("# Azure Deployment Plan for myapp Project", result.Message);
        Assert.Contains("Azure Web App Service", result.Message);
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
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.OK, result.Status);
        Assert.NotNull(result.Message);
        Assert.Contains("# Azure Deployment Plan for k8s-app Project", result.Message);
        Assert.Contains("Azure Kubernetes Service", result.Message);
        Assert.Contains("Provision Azure Infrastructure with Azure CLI", result.Message);
        Assert.Contains("terraform", result.Message); // Default IaC option for aks
        Assert.Contains("Azure Kubernetes Service Deployment", result.Message);
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
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.OK, result.Status);
        Assert.NotNull(result.Message);
        Assert.Contains("# Azure Deployment Plan for default-app Project", result.Message);
        Assert.Contains("Azure Container Apps", result.Message); // Should default to Container Apps
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
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.OK, result.Status);
        Assert.NotNull(result.Message);
        Assert.Contains("# Azure Deployment Plan for default-app Project", result.Message);
        Assert.Contains("Azure Container Apps", result.Message); // Should default to Container Apps
        Assert.Contains("Containerization", result.Message);
        Assert.Contains("Check Azure resources existence", result.Message);
        Assert.Contains("Azure Container Registry", result.Message);
        Assert.Contains("**Existing Azure Resources**", result.Message);
    }
}
