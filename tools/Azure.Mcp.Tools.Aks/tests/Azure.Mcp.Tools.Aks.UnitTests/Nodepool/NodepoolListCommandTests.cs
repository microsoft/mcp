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

public sealed class NodepoolListCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IAksService _aksService;
    private readonly ILogger<NodepoolListCommand> _logger;
    private readonly NodepoolListCommand _command;

    public NodepoolListCommandTests()
    {
        _aksService = Substitute.For<IAksService>();
        _logger = Substitute.For<ILogger<NodepoolListCommand>>();

        var collection = new ServiceCollection();
        collection.AddSingleton(_aksService);
        _serviceProvider = collection.BuildServiceProvider();

        _command = new(_logger);
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
    [InlineData("--subscription sub123 --resource-group rg1 --cluster c1", true)]
    [InlineData("--subscription sub123 --resource-group rg1 --cluster c1 --tenant t1", true)]
    [InlineData("--subscription sub123 --resource-group rg1", false)] // missing cluster
    [InlineData("--subscription sub123 --cluster c1", false)] // missing rg
    [InlineData("--resource-group rg1 --cluster c1", false)] // missing subscription
    [InlineData("", false)]
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        // Arrange
        if (shouldSucceed)
        {
            var testNodePools = new List<Models.NodePool>
            {
                new() { Name = "np1", NodeCount = 3, NodeVmSize = "Standard_DS2_v2" },
                new() { Name = "np2", NodeCount = 5, NodeVmSize = "Standard_D4s_v5" }
            };
            _aksService.ListNodePools(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>())
                .Returns(testNodePools);
        }

        var context = new CommandContext(_serviceProvider);
        var parseResult = _command.GetCommand().Parse(args);

        // Act
        var response = await _command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.Equal(shouldSucceed ? 200 : 400, response.Status);
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
    public async Task ExecuteAsync_ReturnsNodePoolsList()
    {
        // Arrange
        var expectedNodePools = new List<Models.NodePool>
        {
            new() { Name = "systempool", NodeCount = 3, NodeVmSize = "Standard_DS2_v2", Mode = "System" },
            new() { Name = "userpool", NodeCount = 5, NodeVmSize = "Standard_D4s_v5", Mode = "User" }
        };
        _aksService.ListNodePools(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>())
            .Returns(expectedNodePools);

        var context = new CommandContext(_serviceProvider);
        var parseResult = _command.GetCommand().Parse("--subscription sub123 --resource-group rg1 --cluster c1");

        // Act
        var response = await _command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.Equal(200, response.Status);
        Assert.NotNull(response.Results);

        // Verify the mock was called
        await _aksService.Received(1).ListNodePools(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>());

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, AksJsonContext.Default.NodepoolListCommandResult);

        Assert.NotNull(result);
        Assert.Equal(expectedNodePools.Count, result.NodePools.Count);
        Assert.Equal(expectedNodePools[0].Name, result.NodePools[0].Name);
        Assert.Equal(expectedNodePools[0].NodeCount, result.NodePools[0].NodeCount);
        Assert.Equal(expectedNodePools[0].NodeVmSize, result.NodePools[0].NodeVmSize);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsNullWhenNoNodePools()
    {
        // Arrange
        _aksService.ListNodePools(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>())
            .Returns(new List<Models.NodePool>());

        var context = new CommandContext(_serviceProvider);
        var parseResult = _command.GetCommand().Parse("--subscription sub123 --resource-group rg1 --cluster c1");

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
        _aksService.ListNodePools(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>())
            .Returns(Task.FromException<List<Models.NodePool>>(new Exception("Test error")));

        var context = new CommandContext(_serviceProvider);
        var parseResult = _command.GetCommand().Parse("--subscription sub123 --resource-group rg1 --cluster c1");

        // Act
        var response = await _command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.Equal(500, response.Status);
        Assert.Contains("Test error", response.Message);
        Assert.Contains("troubleshooting", response.Message);
    }
}
