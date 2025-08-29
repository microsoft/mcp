// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Tools.Deploy.Commands.Pipeline;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.Deploy.UnitTests.Commands.Pipeline;


public class GuidanceGetCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<GuidanceGetCommand> _logger;
    private readonly Command _commandDefinition;
    private readonly CommandContext _context;
    private readonly GuidanceGetCommand _command;

    public GuidanceGetCommandTests()
    {
        _logger = Substitute.For<ILogger<GuidanceGetCommand>>();

        var collection = new ServiceCollection();
        _serviceProvider = collection.BuildServiceProvider();
        _context = new(_serviceProvider);
        _command = new(_logger);
        _commandDefinition = _command.GetCommand();
    }

    [Fact]
    public async Task Should_generate_pipeline()
    {
        // arrange
        var args = _commandDefinition.Parse([
            "--subscription", "test-subscription-id",
            "--use-azd-pipeline-config", "true"
        ]);

        // act
        var result = await _command.ExecuteAsync(_context, args);

        // assert
        Assert.NotNull(result);
        Assert.Equal(200, result.Status);
        Assert.NotNull(result.Message);
        Assert.Contains("Run \"azd pipeline config\" to help the user create a deployment pipeline.", result.Message);
    }

    [Fact]
    public async Task Should_generate_pipeline_with_github_details()
    {
        // arrange
        var args = _commandDefinition.Parse([
            "--subscription", "test-subscription-id",
            "--use-azd-pipeline-config", "false",
            "--organization-name", "test-org",
            "--repository-name", "test-repo",
            "--github-environment-name", "production"
        ]);

        // act
        var result = await _command.ExecuteAsync(_context, args);

        // assert
        Assert.NotNull(result);
        Assert.Equal(200, result.Status);
        Assert.NotNull(result.Message);
        Assert.Contains("Help the user to set up a CI/CD pipeline", result.Message);
        Assert.Contains("test-org", result.Message);
        Assert.Contains("test-repo", result.Message);
        Assert.Contains("production", result.Message);
    }

    [Fact]
    public async Task Should_generate_pipeline_with_default_azd_pipeline_config()
    {
        // arrange - not providing use-azd-pipeline-config should default to false
        var args = _commandDefinition.Parse([
            "--subscription", "test-subscription-id"
        ]);

        // act
        var result = await _command.ExecuteAsync(_context, args);

        // assert
        Assert.NotNull(result);
        Assert.Equal(200, result.Status);
        Assert.NotNull(result.Message);
        Assert.Contains("Help the user to set up a CI/CD pipeline", result.Message);
        Assert.Contains("Github Actions workflow", result.Message);
    }

    [Fact]
    public async Task Should_generate_pipeline_with_minimal_github_info()
    {
        // arrange
        var args = _commandDefinition.Parse([
            "--subscription", "test-subscription-id",
            "--use-azd-pipeline-config", "false"
        ]);

        // act
        var result = await _command.ExecuteAsync(_context, args);

        // assert
        Assert.NotNull(result);
        Assert.Equal(200, result.Status);
        Assert.NotNull(result.Message);
        Assert.Contains("Help the user to set up a CI/CD pipeline", result.Message);
        Assert.Contains("{$organization-of-repo}", result.Message);
        Assert.Contains("{$repository-name}", result.Message);
        Assert.Contains("dev", result.Message); // default environment
    }

    [Fact]
    public async Task Should_handle_guid_subscription_id()
    {
        // arrange
        var guidSubscriptionId = "12345678-1234-1234-1234-123456789abc";
        var args = _commandDefinition.Parse([
            "--subscription", guidSubscriptionId,
            "--use-azd-pipeline-config", "false"
        ]);

        // act
        var result = await _command.ExecuteAsync(_context, args);

        // assert
        Assert.NotNull(result);
        Assert.Equal(200, result.Status);
        Assert.NotNull(result.Message);
        Assert.Contains($"User is deploying to subscription {guidSubscriptionId}", result.Message);
    }

    [Fact]
    public async Task Should_handle_non_guid_subscription_id()
    {
        // arrange
        var args = _commandDefinition.Parse([
            "--subscription", "my-subscription-name",
            "--use-azd-pipeline-config", "false"
        ]);

        // act
        var result = await _command.ExecuteAsync(_context, args);

        // assert
        Assert.NotNull(result);
        Assert.Equal(200, result.Status);
        Assert.NotNull(result.Message);
        Assert.Contains("az account show --query id -o tsv", result.Message);
    }

    [Fact]
    public async Task Should_include_service_principal_creation_steps()
    {
        // arrange
        var args = _commandDefinition.Parse([
            "--subscription", "test-subscription-id",
            "--use-azd-pipeline-config", "false"
        ]);

        // act
        var result = await _command.ExecuteAsync(_context, args);

        // assert
        Assert.NotNull(result);
        Assert.Equal(200, result.Status);
        Assert.NotNull(result.Message);
        Assert.Contains("az ad sp create-for-rbac", result.Message);
        Assert.Contains("federated-credential create", result.Message);
        Assert.Contains("gh secret set", result.Message);
    }
}
