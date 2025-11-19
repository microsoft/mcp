// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.Policy.Commands;
using Azure.Mcp.Tools.Policy.Commands.Assignment;
using Azure.Mcp.Tools.Policy.Models;
using Azure.Mcp.Tools.Policy.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Policy.UnitTests.Assignment;

public class PolicyAssignmentListCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IPolicyService _service;
    private readonly ILogger<PolicyAssignmentListCommand> _logger;

    public PolicyAssignmentListCommandTests()
    {
        _service = Substitute.For<IPolicyService>();
        _logger = Substitute.For<ILogger<PolicyAssignmentListCommand>>();

        var services = new ServiceCollection();
        services.AddSingleton(_service);
        _serviceProvider = services.BuildServiceProvider();
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        // Arrange & Act
        var command = new PolicyAssignmentListCommand(_logger);

        // Assert
        Assert.NotNull(command);
        Assert.Equal("list", command.Name);
        Assert.Equal("List Policy Assignments", command.Title);
        Assert.Contains("policy assignment", command.Description.ToLower());
        Assert.False(command.Metadata.Destructive);
        Assert.True(command.Metadata.Idempotent);
        Assert.False(command.Metadata.OpenWorld);
        Assert.True(command.Metadata.ReadOnly);
        Assert.False(command.Metadata.LocalRequired);
        Assert.False(command.Metadata.Secret);
    }

    [Theory]
    [InlineData("", "", false, "missing required options")]
    [InlineData("test-sub", "", true, null)]
    [InlineData("test-sub", "/subscriptions/test-sub/resourceGroups/test-rg", true, null)]
    public async Task ExecuteAsync_ValidatesInputCorrectly(
        string subscription,
        string scope,
        bool shouldSucceed,
        string? expectedErrorContext)
    {
        // Arrange
        var command = new PolicyAssignmentListCommand(_logger);
        var args = new List<string>();

        if (!string.IsNullOrEmpty(subscription))
            args.AddRange(["--subscription", subscription]);
        if (!string.IsNullOrEmpty(scope))
            args.AddRange(["--scope", scope]);

        var parseResult = command.GetCommand().Parse([.. args]);
        var context = new CommandContext(_serviceProvider);

        if (shouldSucceed)
        {
            _service.ListPolicyAssignmentsAsync(
                Arg.Any<string>(),
                Arg.Any<string?>(),
                Arg.Any<string?>(),
                Arg.Any<RetryPolicyOptions?>(),
                Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(new List<PolicyAssignment>()));
        }

        // Act
        var response = await command.ExecuteAsync(context, parseResult);

        // Assert
        if (shouldSucceed)
        {
            Assert.NotEqual(HttpStatusCode.BadRequest, response.Status);
        }
        else
        {
            Assert.Equal(HttpStatusCode.BadRequest, response.Status);
            if (expectedErrorContext != null)
            {
                Assert.Contains(expectedErrorContext, response.Message, StringComparison.OrdinalIgnoreCase);
            }
        }
    }

    [Fact]
    public async Task ExecuteAsync_DeserializationValidation()
    {
        // Arrange
        var command = new PolicyAssignmentListCommand(_logger);

        var assignments = new List<PolicyAssignment>
        {
            new()
            {
                Id = "/subscriptions/test-sub/providers/Microsoft.Authorization/policyAssignments/test-assignment",
                Name = "test-assignment",
                DisplayName = "Test Assignment",
                PolicyDefinitionId = "/providers/Microsoft.Authorization/policyDefinitions/test-policy",
                Scope = "/subscriptions/test-sub"
            }
        };

        _service.ListPolicyAssignmentsAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(assignments));

        var parseResult = command.GetCommand().Parse(["--subscription", "test-sub"]);
        var context = new CommandContext(_serviceProvider);

        // Act
        var response = await command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var deserialized = JsonSerializer.Deserialize(json, PolicyJsonContext.Default.PolicyAssignmentListCommandResult);

        Assert.NotNull(deserialized);
        Assert.Single(deserialized.Assignments);
        Assert.Equal("test-assignment", deserialized.Assignments[0].Name);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        // Arrange
        var command = new PolicyAssignmentListCommand(_logger);

        _service.ListPolicyAssignmentsAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Service error"));

        var parseResult = command.GetCommand().Parse(["--subscription", "test-sub"]);
        var context = new CommandContext(_serviceProvider);

        // Act
        var response = await command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.NotNull(response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_WithScope_PassesScopeToService()
    {
        // Arrange
        var command = new PolicyAssignmentListCommand(_logger);

        var assignments = new List<PolicyAssignment>();
        _service.ListPolicyAssignmentsAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(assignments));

        var scope = "/subscriptions/test-sub/resourceGroups/test-rg";
        var parseResult = command.GetCommand().Parse(["--subscription", "test-sub", "--scope", scope]);
        var context = new CommandContext(_serviceProvider);

        // Act
        await command.ExecuteAsync(context, parseResult);

        // Assert
        await _service.Received(1).ListPolicyAssignmentsAsync(
            "test-sub",
            scope,
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WithoutScope_PassesNullScope()
    {
        // Arrange
        var command = new PolicyAssignmentListCommand(_logger);

        var assignments = new List<PolicyAssignment>();
        _service.ListPolicyAssignmentsAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(assignments));

        var parseResult = command.GetCommand().Parse(["--subscription", "test-sub"]);
        var context = new CommandContext(_serviceProvider);

        // Act
        await command.ExecuteAsync(context, parseResult);

        // Assert
        await _service.Received(1).ListPolicyAssignmentsAsync(
            "test-sub",
            null,
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsEmptyList_WhenNoAssignments()
    {
        // Arrange
        var command = new PolicyAssignmentListCommand(_logger);

        var assignments = new List<PolicyAssignment>();
        _service.ListPolicyAssignmentsAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(assignments));

        var parseResult = command.GetCommand().Parse(["--subscription", "test-sub"]);
        var context = new CommandContext(_serviceProvider);

        // Act
        var response = await command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var deserialized = JsonSerializer.Deserialize(json, PolicyJsonContext.Default.PolicyAssignmentListCommandResult);

        Assert.NotNull(deserialized);
        Assert.Empty(deserialized.Assignments);
    }

    [Fact]
    public void GetCommand_ReturnsValidCommand()
    {
        // Arrange
        var command = new PolicyAssignmentListCommand(_logger);

        // Act
        var systemCommand = command.GetCommand();

        // Assert
        Assert.NotNull(systemCommand);
        Assert.Equal("list", systemCommand.Name);
        Assert.NotNull(systemCommand.Description);
    }
}
