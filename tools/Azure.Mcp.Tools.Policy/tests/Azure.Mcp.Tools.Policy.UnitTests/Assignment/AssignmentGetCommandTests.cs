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
    public async Task ExecuteAsync_NoParameters_ReturnsAssignments()
    {
        // Arrange
        var subscription = "sub123";
        var assignments = new List<PolicyAssignment>
        {
            new()
            {
                Id = "/subscriptions/sub1/providers/Microsoft.Authorization/policyAssignments/policy1",
                Name = "policy1",
                DisplayName = "Test Policy",
                EnforcementMode = "Default"
            }
        };

        _service.GetAssignmentsAsync(
            Arg.Is(subscription),
            Arg.Is<string?>(s => string.IsNullOrEmpty(s)),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>()).Returns(assignments);

        var args = _commandDefinition.Parse(["--subscription", subscription]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);
        Assert.Equal(System.Net.HttpStatusCode.OK, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_WithAssignment_ReturnsSpecificAssignment()
    {
        // Arrange
        var subscription = "sub123";
        var assignmentName = "policy1";
        var assignments = new List<PolicyAssignment>
        {
            new()
            {
                Id = $"/subscriptions/{subscription}/providers/Microsoft.Authorization/policyAssignments/{assignmentName}",
                Name = assignmentName,
                DisplayName = "Test Policy Assignment",
                Description = "Test Description",
                EnforcementMode = "Default",
                PolicyDefinitionId = "/providers/Microsoft.Authorization/policyDefinitions/test-def"
            }
        };

        _service.GetAssignmentsAsync(
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>()).Returns(assignments);

        var args = _commandDefinition.Parse(["--subscription", subscription, "--assignment", assignmentName]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);
        Assert.Equal(System.Net.HttpStatusCode.OK, response.Status);

        var json = JsonSerializer.Serialize(response.Results);
        Assert.NotNull(json);
        Assert.Contains(assignmentName, json);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsEmpty_WhenNoAssignments()
    {
        // Arrange
        var subscription = "sub123";

        _service.GetAssignmentsAsync(
            Arg.Is(subscription),
            Arg.Is<string?>(s => string.IsNullOrEmpty(s)),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>()).Returns([]);

        var args = _commandDefinition.Parse(["--subscription", subscription]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);
        Assert.Equal(System.Net.HttpStatusCode.OK, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        // Arrange
        var subscription = "sub123";

        _service.GetAssignmentsAsync(
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Azure CLI command failed"));

        var args = _commandDefinition.Parse(["--subscription", subscription]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotEqual(System.Net.HttpStatusCode.OK, response.Status);
        Assert.Contains("policy assignments", response.Message ?? string.Empty);
    }
}
