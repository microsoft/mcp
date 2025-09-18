// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Text.Json;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.Monitor.Commands;
using Azure.Mcp.Tools.Monitor.Commands.Workspace;
using Azure.Mcp.Tools.Monitor.Models;
using Azure.Mcp.Tools.Monitor.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.Monitor.UnitTests.Workspace;

[Trait("Area", "Monitor")]
public sealed class WorkspaceListCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IMonitorService _monitorService;
    private readonly ILogger<WorkspaceListCommand> _logger;
    private readonly WorkspaceListCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;


    private const string _knownSubscription = "knownSubscription";

    public WorkspaceListCommandTests()
    {
        _monitorService = Substitute.For<IMonitorService>();
        _logger = Substitute.For<ILogger<WorkspaceListCommand>>();

        var collection = new ServiceCollection();
        collection.AddSingleton(_monitorService);
        _serviceProvider = collection.BuildServiceProvider();

        _command = new(_logger);
        _context = new(_serviceProvider);
        _commandDefinition = _command.GetCommand();
    }

    [Theory]
    [InlineData($"--subscription {_knownSubscription}", true)]
    [InlineData($"--subscription {_knownSubscription} --tenant tenant123", true)]
    [InlineData("", false)]
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        // Arrange
        if (shouldSucceed)
        {
            var testWorkspaces = new List<WorkspaceInfo>
            {
                new() { Name = "workspace1", CustomerId = "guid1" },
                new() { Name = "workspace2", CustomerId = "guid2" }
            };
            _monitorService.ListWorkspaces(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>())
                .Returns(testWorkspaces);
        }

        // Act
        var response = await _command.ExecuteAsync(_context, _commandDefinition.Parse(args));

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
    public async Task ExecuteAsync_ReturnsWorkspacesList()
    {
        // Arrange
        var expectedWorkspaces = new List<WorkspaceInfo>
        {
            new() { Name = "workspace1", CustomerId = "guid1" },
            new() { Name = "workspace2", CustomerId = "guid2" },
            new() { Name = "workspace3", CustomerId = "guid3" }
        };
        _monitorService.ListWorkspaces(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>())
            .Returns(expectedWorkspaces);

        // Act
        var response = await _command.ExecuteAsync(_context, _commandDefinition.Parse($"--subscription {_knownSubscription}"));

        // Assert
        Assert.Equal(200, response.Status);
        Assert.NotNull(response.Results);

        // Verify the mock was called
        await _monitorService.Received(1).ListWorkspaces(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>());

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, MonitorJsonContext.Default.WorkspaceListCommandResult);

        Assert.NotNull(result);
        Assert.Equal(expectedWorkspaces.Count, result.Workspaces.Count);
        Assert.Equal(expectedWorkspaces[0].Name, result.Workspaces[0].Name);
        Assert.Equal(expectedWorkspaces[0].CustomerId, result.Workspaces[0].CustomerId);
        Assert.Equal(expectedWorkspaces[1].Name, result.Workspaces[1].Name);
        Assert.Equal(expectedWorkspaces[1].CustomerId, result.Workspaces[1].CustomerId);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsEmptyWhenNoWorkspaces()
    {
        // Arrange
        _monitorService.ListWorkspaces(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>())
            .Returns([]);

        // Act
        var response = await _command.ExecuteAsync(_context, _commandDefinition.Parse($"--subscription {_knownSubscription}"));

        // Assert
        Assert.Equal(200, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, MonitorJsonContext.Default.WorkspaceListCommandResult);

        Assert.NotNull(result);
        Assert.Empty(result.Workspaces);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        // Arrange
        _monitorService.ListWorkspaces(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>())
            .Returns(Task.FromException<List<WorkspaceInfo>>(new Exception("Test error")));

        // Act
        var response = await _command.ExecuteAsync(_context, _commandDefinition.Parse($"--subscription {_knownSubscription}"));

        // Assert
        Assert.Equal(500, response.Status);
        Assert.Contains("Test error", response.Message);
        Assert.Contains("troubleshooting", response.Message);
    }
}
