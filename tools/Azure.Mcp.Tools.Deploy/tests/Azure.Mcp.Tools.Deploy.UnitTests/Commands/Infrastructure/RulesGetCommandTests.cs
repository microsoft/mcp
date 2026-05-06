// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.Deploy.Commands;
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
        var rules = ValidateAndDeserializeResponse(result, DeployJsonContext.Default.RulesGetCommandResult);
        Assert.Contains("Deployment Tool azd rules", rules.Rules, StringComparison.OrdinalIgnoreCase);
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
        var rules = ValidateAndDeserializeResponse(result, DeployJsonContext.Default.RulesGetCommandResult);
        Assert.Contains("main.tf", rules.Rules, StringComparison.OrdinalIgnoreCase);
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
        var rules = ValidateAndDeserializeResponse(result, DeployJsonContext.Default.RulesGetCommandResult);
        Assert.Contains("Additional requirements for Function Apps", rules.Rules, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Storage Blob Data Owner", rules.Rules, StringComparison.OrdinalIgnoreCase);
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
        var rules = ValidateAndDeserializeResponse(result, DeployJsonContext.Default.RulesGetCommandResult);
        Assert.Contains("Additional requirements for Container Apps", rules.Rules, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("mcr.microsoft.com/azuredocs/containerapps-helloworld:latest", rules.Rules, StringComparison.OrdinalIgnoreCase);
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
        var rules = ValidateAndDeserializeResponse(result, DeployJsonContext.Default.RulesGetCommandResult);
        Assert.Contains("The script should be idempotent", rules.Rules, StringComparison.OrdinalIgnoreCase);
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
        var rules = ValidateAndDeserializeResponse(result, DeployJsonContext.Default.RulesGetCommandResult);
        Assert.Contains("Deployment Tool azd rules", rules.Rules, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("IaC Type: bicep rules", rules.Rules, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("main.bicep", rules.Rules, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("No IaC is used", rules.Rules, StringComparison.OrdinalIgnoreCase);
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
        var rules = ValidateAndDeserializeResponse(result, DeployJsonContext.Default.RulesGetCommandResult);
        Assert.Contains("Tools needed:", rules.Rules, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("az cli", rules.Rules, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("azd", rules.Rules, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("docker", rules.Rules, StringComparison.OrdinalIgnoreCase);
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
        var rules = ValidateAndDeserializeResponse(result, DeployJsonContext.Default.RulesGetCommandResult);
        Assert.Contains("Resources: appservice, containerapp, function", rules.Rules, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Additional requirements for App Service", rules.Rules, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Additional requirements for Container Apps", rules.Rules, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Additional requirements for Function Apps", rules.Rules, StringComparison.OrdinalIgnoreCase);
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
        var rules = ValidateAndDeserializeResponse(result, DeployJsonContext.Default.RulesGetCommandResult);
        Assert.Contains("Resources: appservice, containerapp, function, aks, azuredatabaseforpostgresql, azuredatabaseformysql, azuresqldatabase, azurecosmosdb, azurestorageaccount, azurekeyvault", rules.Rules, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("{{", rules.Rules, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("}}", rules.Rules, StringComparison.OrdinalIgnoreCase);
    }
}
