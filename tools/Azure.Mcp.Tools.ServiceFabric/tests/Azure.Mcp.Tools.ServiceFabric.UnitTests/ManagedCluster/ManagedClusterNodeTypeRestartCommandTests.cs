// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
using System.Text.Json;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.ServiceFabric.Commands;
using Azure.Mcp.Tools.ServiceFabric.Commands.ManagedCluster;
using Azure.Mcp.Tools.ServiceFabric.Models;
using Azure.Mcp.Tools.ServiceFabric.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Models.Command;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.ServiceFabric.UnitTests.ManagedCluster;

public class ManagedClusterNodeTypeRestartCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IServiceFabricService _serviceFabricService;
    private readonly ILogger<ManagedClusterNodeTypeRestartCommand> _logger;
    private readonly ManagedClusterNodeTypeRestartCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public ManagedClusterNodeTypeRestartCommandTests()
    {
        _serviceFabricService = Substitute.For<IServiceFabricService>();
        _logger = Substitute.For<ILogger<ManagedClusterNodeTypeRestartCommand>>();

        var collection = new ServiceCollection().AddSingleton(_serviceFabricService);
        _serviceProvider = collection.BuildServiceProvider();
        _command = new(_logger);
        _context = new(_serviceProvider);
        _commandDefinition = _command.GetCommand();
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = _command.GetCommand();
        Assert.Equal("restart", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Theory]
    [InlineData("--subscription sub1 --resource-group rg1 --cluster cluster1 --node-type Worker --nodes Worker_0", true)]
    [InlineData("--subscription sub1 --resource-group rg1 --cluster cluster1 --node-type Worker --nodes Worker_0 --nodes Worker_1", true)]
    [InlineData("--subscription sub1 --resource-group rg1 --cluster cluster1 --node-type Worker --nodes Worker_0 --update-type ByUpgradeDomain", true)]
    [InlineData("--subscription sub1 --resource-group rg1 --cluster cluster1 --nodes Worker_0", false)] // Missing node-type
    [InlineData("--subscription sub1 --resource-group rg1 --node-type Worker --nodes Worker_0", false)] // Missing cluster
    [InlineData("--subscription sub1 --cluster cluster1 --node-type Worker --nodes Worker_0", false)] // Missing resource-group
    [InlineData("--resource-group rg1 --cluster cluster1 --node-type Worker --nodes Worker_0", false)] // Missing subscription
    [InlineData("--subscription sub1 --resource-group rg1 --cluster cluster1 --node-type Worker", false)] // Missing nodes
    [InlineData("", false)] // Missing all required options
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        // Arrange
        if (shouldSucceed)
        {
            _serviceFabricService.RestartManagedClusterNodes(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string[]>(),
                Arg.Any<string>(),
                Arg.Any<string?>(),
                Arg.Any<RetryPolicyOptions>(),
                Arg.Any<CancellationToken>())
                .Returns(new RestartNodeResponse { StatusCode = 202 });
        }

        var context = new CommandContext(_serviceProvider);
        var parseResult = _command.GetCommand().Parse(args);

        // Act
        var response = await _command.ExecuteAsync(context, parseResult, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(shouldSucceed ? HttpStatusCode.OK : HttpStatusCode.BadRequest, response.Status);
        if (shouldSucceed)
        {
            Assert.NotNull(response.Results);
            Assert.Equal("Success", response.Message);
        }
        else
        {
            Assert.Contains("required", response.Message.ToLower());
        }
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsRestartResponse()
    {
        // Arrange
        var expectedResponse = new RestartNodeResponse
        {
            StatusCode = 202,
            AsyncOperationUrl = "https://management.azure.com/subscriptions/sub1/providers/Microsoft.ServiceFabric/locations/eastus/managedClusterOperationResults/op-id",
            Location = "https://management.azure.com/subscriptions/sub1/resourceGroups/rg1/providers/Microsoft.ServiceFabric/managedClusters/cluster1/nodeTypes/Worker/operationResults/op-id"
        };

        _serviceFabricService.RestartManagedClusterNodes(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string[]>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedResponse);

        var context = new CommandContext(_serviceProvider);
        var parseResult = _command.GetCommand().Parse(["--subscription", "sub1", "--resource-group", "rg1", "--cluster", "cluster1", "--node-type", "Worker", "--nodes", "Worker_0", "--nodes", "Worker_1"]);

        // Act
        var response = await _command.ExecuteAsync(context, parseResult, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        await _serviceFabricService.Received(1).RestartManagedClusterNodes(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string[]>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>());

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, ServiceFabricJsonContext.Default.ManagedClusterNodeTypeRestartCommandResult);

        Assert.NotNull(result);
        Assert.Equal(202, result.Response.StatusCode);
        Assert.Equal(expectedResponse.AsyncOperationUrl, result.Response.AsyncOperationUrl);
        Assert.Equal(expectedResponse.Location, result.Response.Location);
    }

    [Fact]
    public async Task ExecuteAsync_PassesUpdateTypeToService()
    {
        // Arrange
        _serviceFabricService.RestartManagedClusterNodes(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string[]>(),
            Arg.Is("ByUpgradeDomain"),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(new RestartNodeResponse { StatusCode = 202 });

        var context = new CommandContext(_serviceProvider);
        var parseResult = _command.GetCommand().Parse("--subscription sub1 --resource-group rg1 --cluster cluster1 --node-type Worker --nodes Worker_0 --update-type ByUpgradeDomain");

        // Act
        var response = await _command.ExecuteAsync(context, parseResult, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        await _serviceFabricService.Received(1).RestartManagedClusterNodes(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string[]>(),
            Arg.Is("ByUpgradeDomain"),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_DeserializationValidation()
    {
        // Arrange
        _serviceFabricService.RestartManagedClusterNodes(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string[]>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(new RestartNodeResponse { StatusCode = 202 });

        var parseResult = _commandDefinition.Parse("--subscription sub1 --resource-group rg1 --cluster cluster1 --node-type Worker --nodes Worker_0");

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, ServiceFabricJsonContext.Default.ManagedClusterNodeTypeRestartCommandResult);

        Assert.NotNull(result);
        Assert.Equal(202, result.Response.StatusCode);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        // Arrange
        _serviceFabricService.RestartManagedClusterNodes(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string[]>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromException<RestartNodeResponse>(new Exception("Test error")));

        var parseResult = _commandDefinition.Parse("--subscription sub1 --resource-group rg1 --cluster cluster1 --node-type Worker --nodes Worker_0");

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test error", response.Message);
        Assert.Contains("troubleshooting", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesNotFoundError()
    {
        // Arrange
        _serviceFabricService.RestartManagedClusterNodes(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string[]>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromException<RestartNodeResponse>(
                new HttpRequestException("Not found", null, HttpStatusCode.NotFound)));

        var parseResult = _commandDefinition.Parse("--subscription sub1 --resource-group rg1 --cluster cluster1 --node-type Worker --nodes Worker_0");

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.Status);
        Assert.Contains("not found", response.Message.ToLower());
    }

    [Fact]
    public async Task BindOptions_BindsOptionsCorrectly()
    {
        // Arrange
        _serviceFabricService.RestartManagedClusterNodes(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string[]>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(new RestartNodeResponse { StatusCode = 202 });

        var parseResult = _commandDefinition.Parse(["--subscription", "sub1", "--resource-group", "rg1", "--cluster", "cluster1", "--node-type", "Worker", "--nodes", "Worker_0", "--nodes", "Worker_1", "--update-type", "ByUpgradeDomain"]);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        await _serviceFabricService.Received(1).RestartManagedClusterNodes(
            Arg.Is("sub1"),
            Arg.Is("rg1"),
            Arg.Is("cluster1"),
            Arg.Is("Worker"),
            Arg.Is<string[]>(n => n.Length == 2 && n[0] == "Worker_0" && n[1] == "Worker_1"),
            Arg.Is("ByUpgradeDomain"),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>());
    }
}
