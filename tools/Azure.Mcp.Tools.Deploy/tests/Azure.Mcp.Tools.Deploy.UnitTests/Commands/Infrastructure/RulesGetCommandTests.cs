// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Tools.Deploy.Commands.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.Deploy.UnitTests.Commands.Infrastructure;


public class RulesGetCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<RulesGetCommand> _logger;
    private readonly Command _commandDefinition;
    private readonly CommandContext _context;
    private readonly RulesGetCommand _command;

    public RulesGetCommandTests()
    {
        _logger = Substitute.For<ILogger<RulesGetCommand>>();

        var collection = new ServiceCollection();
        _serviceProvider = collection.BuildServiceProvider();
        _context = new(_serviceProvider);
        _command = new(_logger);
        _commandDefinition = _command.GetCommand();
    }

    [Fact]
    public async Task Should_get_infrastructure_code_rules()
    {
        // arrange
        var args = _commandDefinition.Parse([
            "--deployment-tool", "azd",
            "--iac-type", "bicep",
            "--resource-types", "appservice, azurestorage"
        ]);

        // act
        var result = await _command.ExecuteAsync(_context, args);

        // assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.OK, result.Status);
        Assert.NotNull(result.Message);
        Assert.Contains("Deployment Tool azd rules", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Should_get_infrastructure_rules_for_terraform()
    {
        // arrange
        var args = _commandDefinition.Parse([
            "--deployment-tool", "azd",
            "--iac-type", "terraform",
            "--resource-types", "containerapp, azurecosmosdb"
        ]);

        // act
        var result = await _command.ExecuteAsync(_context, args);

        // assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.OK, result.Status);
        Assert.NotNull(result.Message);
        Assert.Contains("Expected parameters in terraform parameters", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Should_get_infrastructure_rules_for_function_app()
    {
        // arrange
        var args = _commandDefinition.Parse([
            "--deployment-tool", "azd",
            "--iac-type", "bicep",
            "--resource-types", "function"
        ]);

        // act
        var result = await _command.ExecuteAsync(_context, args);

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
        // arrange
        var args = _commandDefinition.Parse([
            "--deployment-tool", "azd",
            "--iac-type", "bicep",
            "--resource-types", "containerapp"
        ]);

        // act
        var result = await _command.ExecuteAsync(_context, args);

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
        // arrange
        var args = _commandDefinition.Parse([
            "--deployment-tool", "AzCli",
            "--iac-type", "",
            "--resource-types", "aks"
        ]);

        // act
        var result = await _command.ExecuteAsync(_context, args);

        // assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.OK, result.Status);
        Assert.NotNull(result.Message);
        Assert.Contains("If creating AzCli script, the script should stop if any command fails.", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Should_include_necessary_tools_in_response()
    {
        // arrange
        var args = _commandDefinition.Parse([
            "--deployment-tool", "azd",
            "--iac-type", "terraform",
            "--resource-types", "containerapp"
        ]);

        // act
        var result = await _command.ExecuteAsync(_context, args);

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
        // arrange
        var args = _commandDefinition.Parse([
            "--deployment-tool", "azd",
            "--iac-type", "bicep",
            "--resource-types", "appservice,containerapp,function"
        ]);

        // act
        var result = await _command.ExecuteAsync(_context, args);

        // assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.OK, result.Status);
        Assert.NotNull(result.Message);
        Assert.Contains("Resources: appservice, containerapp, function", result.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("App Service Rules", result.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Additional requirements for Container Apps", result.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Additional requirements for Function Apps", result.Message, StringComparison.OrdinalIgnoreCase);
    }
}
