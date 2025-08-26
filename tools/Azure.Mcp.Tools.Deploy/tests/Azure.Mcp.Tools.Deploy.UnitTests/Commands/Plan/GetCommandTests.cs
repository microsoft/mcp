// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Tools.Deploy.Commands.Plan;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.Deploy.UnitTests.Commands.Plan;


public class GetCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<GetCommand> _logger;
    private readonly Command _commandDefinition;
    private readonly CommandContext _context;
    private readonly GetCommand _command;

    public GetCommandTests()
    {
        _logger = Substitute.For<ILogger<GetCommand>>();

        var collection = new ServiceCollection();
        _serviceProvider = collection.BuildServiceProvider();
        _context = new(_serviceProvider);
        _command = new(_logger);
        _commandDefinition = _command.GetCommand();
    }

    [Fact]
    public async Task GetPlan_Should_Return_Expected_Result()
    {
        // arrange
        var args = _commandDefinition.Parse([
            "--workspace-folder", "C:/",
            "--project-name", "django",
            "--target-app-service", "ContainerApp",
            "--provisioning-tool", "AZD",
            "--azd-iac-options", "bicep"
        ]);

        // act
        var result = await _command.ExecuteAsync(_context, args);

        // assert
        Assert.NotNull(result);
        Assert.Equal(200, result.Status);
        Assert.NotNull(result.Message);
        Assert.Contains("# Azure Deployment Plan for django Project", result.Message);
        Assert.Contains("Azure Container Apps", result.Message);
    }

    [Fact]
    public async Task Should_get_plan_with_default_iac_options()
    {
        // arrange
        var args = _commandDefinition.Parse([
            "--workspace-folder", "C:/test",
            "--project-name", "myapp",
            "--target-app-service", "WebApp",
            "--provisioning-tool", "azd"
            // No azd-iac-options provided - should default to "bicep"
        ]);

        // act
        var result = await _command.ExecuteAsync(_context, args);

        // assert
        Assert.NotNull(result);
        Assert.Equal(200, result.Status);
        Assert.NotNull(result.Message);
        Assert.Contains("# Azure Deployment Plan for myapp Project", result.Message);
        Assert.Contains("Azure Web App Service", result.Message);
    }

    [Fact]
    public async Task Should_get_plan_for_kubernetes()
    {
        // arrange
        var args = _commandDefinition.Parse([
            "--workspace-folder", "C:/k8s-project",
            "--project-name", "k8s-app",
            "--target-app-service", "AKS",
            "--provisioning-tool", "azcli"
        ]);

        // act
        var result = await _command.ExecuteAsync(_context, args);

        // assert
        Assert.NotNull(result);
        Assert.Equal(200, result.Status);
        Assert.NotNull(result.Message);
        Assert.Contains("# Azure Deployment Plan for k8s-app Project", result.Message);
        Assert.Contains("Azure Kubernetes Service", result.Message);
    }

    [Fact]
    public async Task Should_get_plan_with_default_target_service()
    {
        // arrange
        var args = _commandDefinition.Parse([
            "--workspace-folder", "C:/",
            "--project-name", "default-app",
            "--target-app-service", "unknown-service", // This should default to Container Apps
            "--provisioning-tool", "AZD"
        ]);
        // act
        var result = await _command.ExecuteAsync(_context, args);

        // assert
        Assert.NotNull(result);
        Assert.Equal(200, result.Status);
        Assert.NotNull(result.Message);
        Assert.Contains("# Azure Deployment Plan for default-app Project", result.Message);
        Assert.Contains("Azure Container Apps", result.Message); // Should default to Container Apps
    }
}
