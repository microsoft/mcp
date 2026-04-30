// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.Deploy.Commands.Infrastructure;
using Microsoft.Mcp.Tests.Client;
using Xunit;

namespace Azure.Mcp.Tools.Deploy.UnitTests.Commands.Infrastructure;


public class RulesGetCommandTests : CommandUnitTestsBase<RulesGetCommand, object>
{
    [Fact]
    public async Task Should_get_infrastructure_code_rules()
    {
        // arrange & act
        var result = await ExecuteCommandAsync(
            "--deployment-tool", "azd",
            "--iac-type", "bicep",
            "--resource-types", "appservice, azurestorage");

        // assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.OK, result.Status);
        Assert.NotNull(result.Message);
        Assert.Contains("Deployment Tool azd rules", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Should_get_infrastructure_rules_for_terraform()
    {
        // arrange & act
        var result = await ExecuteCommandAsync(
            "--deployment-tool", "azd",
            "--iac-type", "terraform",
            "--resource-types", "containerapp, azurecosmosdb");

        // assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.OK, result.Status);
        Assert.NotNull(result.Message);
        Assert.Contains("main.tf", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Should_get_infrastructure_rules_for_function_app()
    {
        // arrange & act
        var result = await ExecuteCommandAsync(
            "--deployment-tool", "azd",
            "--iac-type", "bicep",
            "--resource-types", "function");

        // assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.OK, result.Status);
        Assert.NotNull(result.Message);
        Assert.Contains("Additional requirements for Function Apps", result.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Storage Blob Data Owner", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Should_get_infrastructure_rules_for_container_app()
    {
        // arrange & act
        var result = await ExecuteCommandAsync(
            "--deployment-tool", "azd",
            "--iac-type", "bicep",
            "--resource-types", "containerapp");

        // assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.OK, result.Status);
        Assert.NotNull(result.Message);
        Assert.Contains("Additional requirements for Container Apps", result.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("mcr.microsoft.com/azuredocs/containerapps-helloworld:latest", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Should_get_infrastructure_rules_for_azcli_deployment_tool()
    {
        // arrange & act
        var result = await ExecuteCommandAsync(
            "--deployment-tool", "AzCli",
            "--iac-type", "",
            "--resource-types", "aks");

        // assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.OK, result.Status);
        Assert.NotNull(result.Message);
        Assert.Contains("The script should be idempotent", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Should_default_to_bicep_for_azd_when_iac_type_is_empty()
    {
        // arrange & act
        var result = await ExecuteCommandAsync(
            "--deployment-tool", "azd",
            "--iac-type", "",
            "--resource-types", "appservice");

        // assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.OK, result.Status);
        Assert.NotNull(result.Message);
        Assert.Contains("Deployment Tool azd rules", result.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("IaC Type: bicep rules", result.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("main.bicep", result.Message, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("No IaC is used", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Should_include_necessary_tools_in_response()
    {
        // arrange & act
        var result = await ExecuteCommandAsync(
            "--deployment-tool", "azd",
            "--iac-type", "terraform",
            "--resource-types", "containerapp");

        // assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.OK, result.Status);
        Assert.NotNull(result.Message);
        Assert.Contains("Tools needed:", result.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("az cli", result.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("azd", result.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("docker", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Should_handle_multiple_resource_types()
    {
        // arrange & act
        var result = await ExecuteCommandAsync(
            "--deployment-tool", "azd",
            "--iac-type", "bicep",
            "--resource-types", "appservice,containerapp,function");

        // assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.OK, result.Status);
        Assert.NotNull(result.Message);
        Assert.Contains("Resources: appservice, containerapp, function", result.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Additional requirements for App Service", result.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Additional requirements for Container Apps", result.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Additional requirements for Function Apps", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Should_handle_azcli_terraform_all_resource_types()
    {
        // arrange & act
        var result = await ExecuteCommandAsync(
            "--deployment-tool", "AzCli",
            "--iac-type", "terraform",
            "--resource-types", "appservice,containerapp,function,aks,azuredatabaseforpostgresql,azuredatabaseformysql,azuresqldatabase,azurecosmosdb,azurestorageaccount,azurekeyvault");

        // assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.OK, result.Status);
        Assert.NotNull(result.Message);
        Assert.Contains("Resources: appservice, containerapp, function, aks, azuredatabaseforpostgresql, azuredatabaseformysql, azuresqldatabase, azurecosmosdb, azurestorageaccount, azurekeyvault", result.Message, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("{{", result.Message, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("}}", result.Message, StringComparison.OrdinalIgnoreCase);
    }
}
