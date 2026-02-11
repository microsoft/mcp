// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
using System.Text.Json;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.ServiceFabric.Commands;
using Azure.Mcp.Tools.ServiceFabric.Commands.ManagedCluster;
using Azure.Mcp.Tools.ServiceFabric.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Models.Command;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.ServiceFabric.UnitTests.ManagedCluster;

public class ManagedClusterNodeListCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IServiceFabricService _serviceFabricService;
    private readonly ILogger<ManagedClusterNodeListCommand> _logger;
    private readonly ManagedClusterNodeListCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public ManagedClusterNodeListCommandTests()
    {
        _serviceFabricService = Substitute.For<IServiceFabricService>();
        _logger = Substitute.For<ILogger<ManagedClusterNodeListCommand>>();

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
        Assert.Equal("list", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Theory]
    [InlineData("--subscription sub1 --resource-group rg1 --cluster cluster1", true)]
    [InlineData("--subscription sub1 --cluster cluster1", false)]  // Missing resource-group
    [InlineData("--subscription sub1 --resource-group rg1", false)] // Missing cluster
    [InlineData("--resource-group rg1 --cluster cluster1", false)] // Missing subscription
    [InlineData("", false)]                                         // Missing all required options
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        // Arrange
        if (shouldSucceed)
        {
            _serviceFabricService.ListManagedClusterNodes(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string?>(),
                Arg.Any<RetryPolicyOptions>(),
                Arg.Any<CancellationToken>())
                .Returns([]);
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
    public async Task ExecuteAsync_ReturnsNodesList()
    {
        // Arrange
        var expectedNodes = new List<Models.ManagedClusterNode>
        {
            new()
            {
                Id = "/subscriptions/sub1/resourcegroups/rg1/providers/Microsoft.ServiceFabric/managedClusters/cluster1/Nodes/primary_0",
                Properties = new()
                {
                    Name = "primary_0",
                    Type = "primary",
                    NodeStatus = 1,
                    IpAddressOrFQDN = "10.0.0.4",
                    FaultDomain = "fd:/0",
                    UpgradeDomain = "0",
                    IsSeedNode = true
                }
            },
            new()
            {
                Id = "/subscriptions/sub1/resourcegroups/rg1/providers/Microsoft.ServiceFabric/managedClusters/cluster1/Nodes/Worker_1",
                Properties = new()
                {
                    Name = "Worker_1",
                    Type = "Worker",
                    NodeStatus = 1,
                    IpAddressOrFQDN = "10.0.0.5",
                    FaultDomain = "fd:/az1/0",
                    UpgradeDomain = "1",
                    IsSeedNode = false
                }
            }
        };

        _serviceFabricService.ListManagedClusterNodes(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedNodes);

        var context = new CommandContext(_serviceProvider);
        var parseResult = _command.GetCommand().Parse("--subscription sub1 --resource-group rg1 --cluster cluster1");

        // Act
        var response = await _command.ExecuteAsync(context, parseResult, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        await _serviceFabricService.Received(1).ListManagedClusterNodes(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>());

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, ServiceFabricJsonContext.Default.ManagedClusterNodeListCommandResult);

        Assert.NotNull(result);
        Assert.Equal(expectedNodes.Count, result.Nodes.Count);
        Assert.Equal(expectedNodes[0].Id, result.Nodes[0].Id);

        var node0Props = result.Nodes[0].Properties!;
        var node1Props = result.Nodes[1].Properties!;
        Assert.Equal("primary_0", node0Props.Name);
        Assert.Equal("primary", node0Props.Type);
        Assert.Equal(1, node0Props.NodeStatus);
        Assert.Equal("10.0.0.4", node0Props.IpAddressOrFQDN);
        Assert.Equal("Worker_1", node1Props.Name);
        Assert.Equal("fd:/az1/0", node1Props.FaultDomain);
        Assert.False(node1Props.IsSeedNode);
    }

    [Fact]
    public async Task ExecuteAsync_DeserializationValidation()
    {
        // Arrange
        _serviceFabricService.ListManagedClusterNodes(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns([]);

        var parseResult = _commandDefinition.Parse("--subscription sub1 --resource-group rg1 --cluster cluster1");

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, ServiceFabricJsonContext.Default.ManagedClusterNodeListCommandResult);

        Assert.NotNull(result);
        Assert.Empty(result.Nodes);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        // Arrange
        _serviceFabricService.ListManagedClusterNodes(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromException<List<Models.ManagedClusterNode>>(new Exception("Test error")));

        var parseResult = _commandDefinition.Parse("--subscription sub1 --resource-group rg1 --cluster cluster1");

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test error", response.Message);
        Assert.Contains("troubleshooting", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsEmptyListWhenNoNodes()
    {
        // Arrange
        _serviceFabricService.ListManagedClusterNodes(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns([]);

        var context = new CommandContext(_serviceProvider);
        var parseResult = _command.GetCommand().Parse("--subscription sub1 --resource-group rg1 --cluster cluster1");

        // Act
        var response = await _command.ExecuteAsync(context, parseResult, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, ServiceFabricJsonContext.Default.ManagedClusterNodeListCommandResult);

        Assert.NotNull(result);
        Assert.Empty(result.Nodes);
    }
}
