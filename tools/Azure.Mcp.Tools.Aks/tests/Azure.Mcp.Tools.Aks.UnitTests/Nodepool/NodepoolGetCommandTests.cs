// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.Aks.Commands;
using Azure.Mcp.Tools.Aks.Commands.Nodepool;
using Azure.Mcp.Tools.Aks.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.Aks.UnitTests.Nodepool;

public sealed class NodepoolGetCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IAksService _aksService;
    private readonly ILogger<NodepoolGetCommand> _logger;
    private readonly NodepoolGetCommand _command;

    public NodepoolGetCommandTests()
    {
        _aksService = Substitute.For<IAksService>();
        _logger = Substitute.For<ILogger<NodepoolGetCommand>>();

        var collection = new ServiceCollection();
        collection.AddSingleton(_aksService);
        _serviceProvider = collection.BuildServiceProvider();

        _command = new(_logger);
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = _command.GetCommand();
        Assert.Equal("get", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Theory]
    [InlineData("--subscription sub123 --resource-group rg1 --cluster c1 --nodepool np1", true)]
    [InlineData("--subscription sub123 --resource-group rg1 --cluster c1 --nodepool np1 --tenant t1", true)]
    [InlineData("--subscription sub123 --resource-group rg1 --cluster c1", false)] // missing nodepool
    [InlineData("--subscription sub123 --resource-group rg1 --nodepool np1", false)] // missing cluster
    [InlineData("--subscription sub123 --cluster c1 --nodepool np1", false)] // missing rg
    [InlineData("--resource-group rg1 --cluster c1 --nodepool np1", false)] // missing subscription
    [InlineData("", false)]
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        // Arrange
        if (shouldSucceed)
        {
            var testNodePool = new Models.NodePool
            {
                Name = "np1",
                NodeCount = 3,
                NodeVmSize = "Standard_DS2_v2",
                Mode = "System"
            };
            _aksService.GetNodePool(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>())
                .Returns(testNodePool);
        }

        var context = new CommandContext(_serviceProvider);
        var parseResult = _command.GetCommand().Parse(args);

        // Act
        var response = await _command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.Equal(shouldSucceed ? 200 : 400, response.Status);
        if (shouldSucceed)
        {
            Assert.Equal("Success", response.Message);
        }
        else
        {
            Assert.Contains("required", response.Message.ToLower());
        }
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsNodePool()
    {
        // Arrange
        var expectedNodePool = new Models.NodePool
        {
            Name = "userpool",
            NodeCount = 5,
            NodeVmSize = "Standard_D4s_v5",
            Mode = "User"
        };
        _aksService.GetNodePool(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>())
            .Returns(expectedNodePool);

        var context = new CommandContext(_serviceProvider);
        var parseResult = _command.GetCommand().Parse("--subscription sub123 --resource-group rg1 --cluster c1 --nodepool userpool");

        // Act
        var response = await _command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.Equal(200, response.Status);
        Assert.NotNull(response.Results);

        await _aksService.Received(1).GetNodePool(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>());

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, AksJsonContext.Default.NodepoolGetCommandResult);

        Assert.NotNull(result);
        Assert.Equal(expectedNodePool.Name, result.NodePool.Name);
        Assert.Equal(expectedNodePool.NodeCount, result.NodePool.NodeCount);
        Assert.Equal(expectedNodePool.NodeVmSize, result.NodePool.NodeVmSize);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsNullWhenNodePoolNotFound()
    {
        // Arrange
        _aksService.GetNodePool(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>())
            .Returns((Models.NodePool?)null);

        var context = new CommandContext(_serviceProvider);
        var parseResult = _command.GetCommand().Parse("--subscription sub123 --resource-group rg1 --cluster c1 --nodepool missing");

        // Act
        var response = await _command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.Equal(200, response.Status);
        Assert.Null(response.Results);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        // Arrange
        _aksService.GetNodePool(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>())
            .Returns(Task.FromException<Models.NodePool?>(new Exception("Test error")));

        var context = new CommandContext(_serviceProvider);
        var parseResult = _command.GetCommand().Parse("--subscription sub123 --resource-group rg1 --cluster c1 --nodepool np1");

        // Act
        var response = await _command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.Equal(500, response.Status);
        Assert.Contains("Test error", response.Message);
        Assert.Contains("troubleshooting", response.Message);
    }
}

