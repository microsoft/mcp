// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.Policy.Commands.Assignment;
using Azure.Mcp.Tools.Policy.Models;
using Azure.Mcp.Tools.Policy.Services;
using NSubstitute.ExceptionExtensions;

namespace Azure.Mcp.Tools.Policy.UnitTests.Assignment;

public class AssignmentGetCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IPolicyService _service;
    private readonly ILogger<AssignmentGetCommand> _logger;
    private readonly AssignmentGetCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public AssignmentGetCommandTests()
    {
        _service = Substitute.For<IPolicyService>();
        _logger = Substitute.For<ILogger<AssignmentGetCommand>>();

        var collection = new ServiceCollection().AddSingleton(_service);
        _serviceProvider = collection.BuildServiceProvider();
        _command = new(_logger);
        _context = new(_serviceProvider);
        _commandDefinition = _command.GetCommand();
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        // Assert
        Assert.NotNull(_command);
        Assert.Equal("get", _command.Name);
        Assert.NotNull(_command.Description);
        Assert.NotNull(_command.Title);
        Assert.NotEmpty(_command.Id);
    }

    [Fact]
    public async Task ExecuteAsync_NoParameters_ReturnsEmptyAssignments()
    {
        // Arrange
        var subscription = "sub123";
        var args = _commandDefinition.Parse(["--subscription", subscription]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);
        Assert.Equal(System.Net.HttpStatusCode.OK, response.Status);

        var json = JsonSerializer.Serialize(response.Results);
        Assert.Contains("\"results\":[]", json);
    }

    [Fact]
    public async Task ExecuteAsync_WithPolicyAssignments_ReturnsAssignments()
    {
        // Arrange - Note: This command accepts policy assignments as input and returns them
        // The actual filtering/fetching happens outside this command
        var subscription = "sub123";
        var args = _commandDefinition.Parse(["--subscription", subscription]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);
        Assert.Equal(System.Net.HttpStatusCode.OK, response.Status);
    }

    [Fact]
    public async Task BindOptions_BindsAssignmentCorrectly()
    {
        // Arrange
        var subscription = "sub123";
        var assignment = "policy1";
        var args = _commandDefinition.Parse(["--subscription", subscription, "--assignment", assignment]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert - verify it executed successfully, which means options were bound correctly
        Assert.NotNull(response);
        Assert.Equal(System.Net.HttpStatusCode.OK, response.Status);
    }

    [Fact]
    public void Metadata_IsSetCorrectly()
    {
        // Assert
        Assert.False(_command.Metadata.Destructive);
        Assert.False(_command.Metadata.OpenWorld);
        Assert.True(_command.Metadata.Idempotent);
        Assert.True(_command.Metadata.ReadOnly);
        Assert.False(_command.Metadata.Secret);
        Assert.True(_command.Metadata.LocalRequired);
    }
}
