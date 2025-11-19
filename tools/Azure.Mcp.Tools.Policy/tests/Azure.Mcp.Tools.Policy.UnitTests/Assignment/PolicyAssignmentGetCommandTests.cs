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

public class PolicyAssignmentGetCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IPolicyService _service;
    private readonly ILogger<PolicyAssignmentGetCommand> _logger;

    public PolicyAssignmentGetCommandTests()
    {
        _service = Substitute.For<IPolicyService>();
        _logger = Substitute.For<ILogger<PolicyAssignmentGetCommand>>();

        var services = new ServiceCollection();
        services.AddSingleton(_service);
        _serviceProvider = services.BuildServiceProvider();
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        // Arrange & Act
        var command = new PolicyAssignmentGetCommand(_logger);

        // Assert
        Assert.NotNull(command);
        Assert.Equal("get", command.Name);
        Assert.Equal("Get Policy Assignment", command.Title);
        Assert.Contains("policy assignment", command.Description.ToLower());
        Assert.False(command.Metadata.Destructive);
        Assert.True(command.Metadata.Idempotent);
        Assert.False(command.Metadata.OpenWorld);
        Assert.True(command.Metadata.ReadOnly);
        Assert.False(command.Metadata.LocalRequired);
        Assert.False(command.Metadata.Secret);
    }

    [Theory]
    [InlineData("", "", "", false, "missing required options")]
    [InlineData("test-assignment", "", "", false, "missing required options")]
    [InlineData("", "/subscriptions/test", "", false, "missing required options")]
    [InlineData("test-assignment", "/subscriptions/test-sub", "test-sub", true, null)]
    public async Task ExecuteAsync_ValidatesInputCorrectly(
        string assignmentName,
        string scope,
        string subscription,
        bool shouldSucceed,
        string? expectedErrorContext)
    {
        // Arrange
        var command = new PolicyAssignmentGetCommand(_logger);
        var args = new List<string>();

        if (!string.IsNullOrEmpty(assignmentName))
            args.AddRange(["--assignment-name", assignmentName]);
        if (!string.IsNullOrEmpty(scope))
            args.AddRange(["--scope", scope]);
        if (!string.IsNullOrEmpty(subscription))
            args.AddRange(["--subscription", subscription]);

        var parseResult = command.GetCommand().Parse([.. args]);
        var context = new CommandContext(_serviceProvider);

        if (shouldSucceed)
        {
            var testAssignment = new PolicyAssignment
            {
                Id = "/subscriptions/test-sub/providers/Microsoft.Authorization/policyAssignments/test-assignment",
                Name = assignmentName,
                DisplayName = "Test Assignment",
                PolicyDefinitionId = "/providers/Microsoft.Authorization/policyDefinitions/test-policy",
                Scope = scope,
                EnforcementMode = "Default"
            };

            _service.GetPolicyAssignmentAsync(
                assignmentName,
                scope,
                subscription,
                Arg.Any<string?>(),
                Arg.Any<RetryPolicyOptions?>(),
                Arg.Any<CancellationToken>())
                .Returns(testAssignment);
        }

        // Act
        var response = await command.ExecuteAsync(context, parseResult);

        // Assert
        if (shouldSucceed)
        {
            Assert.Equal(HttpStatusCode.OK, response.Status);
            Assert.NotNull(response.Results);
        }
        else
        {
            Assert.NotEqual(HttpStatusCode.OK, response.Status);
            if (expectedErrorContext != null)
            {
                Assert.Contains(expectedErrorContext.ToLower(), response.Message.ToLower());
            }
        }
    }

    [Fact]
    public async Task ExecuteAsync_DeserializationValidation()
    {
        // Arrange
        var command = new PolicyAssignmentGetCommand(_logger);
        var args = command.GetCommand().Parse([
            "--assignment-name", "test-assignment",
            "--scope", "/subscriptions/test-sub",
            "--subscription", "test-sub"
        ]);
        var context = new CommandContext(_serviceProvider);

        var testAssignment = new PolicyAssignment
        {
            Id = "/subscriptions/test-sub/providers/Microsoft.Authorization/policyAssignments/test-assignment",
            Name = "test-assignment",
            Type = "Microsoft.Authorization/policyAssignments",
            DisplayName = "Test Assignment",
            PolicyDefinitionId = "/providers/Microsoft.Authorization/policyDefinitions/test-policy",
            Scope = "/subscriptions/test-sub",
            EnforcementMode = "Default",
            Description = "Test policy assignment"
        };

        _service.GetPolicyAssignmentAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(testAssignment);

        // Act
        var response = await command.ExecuteAsync(context, args);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, PolicyJsonContext.Default.PolicyAssignmentGetCommandResult);

        Assert.NotNull(result);
        Assert.NotNull(result.Assignment);
        Assert.Equal("test-assignment", result.Assignment.Name);
        Assert.Equal("Test Assignment", result.Assignment.DisplayName);
        Assert.Equal("/providers/Microsoft.Authorization/policyDefinitions/test-policy", result.Assignment.PolicyDefinitionId);
        Assert.Equal("/subscriptions/test-sub", result.Assignment.Scope);
        Assert.Equal("Default", result.Assignment.EnforcementMode);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        // Arrange
        var command = new PolicyAssignmentGetCommand(_logger);
        var args = command.GetCommand().Parse([
            "--assignment-name", "test-assignment",
            "--scope", "/subscriptions/test-sub",
            "--subscription", "test-sub"
        ]);
        var context = new CommandContext(_serviceProvider);

        _service.GetPolicyAssignmentAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Service error"));

        // Act
        var response = await command.ExecuteAsync(context, args);

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("error", response.Message.ToLower());
    }

    [Fact]
    public async Task ExecuteAsync_HandlesNotFound()
    {
        // Arrange
        var command = new PolicyAssignmentGetCommand(_logger);
        var args = command.GetCommand().Parse([
            "--assignment-name", "nonexistent",
            "--scope", "/subscriptions/test-sub",
            "--subscription", "test-sub"
        ]);
        var context = new CommandContext(_serviceProvider);

        _service.GetPolicyAssignmentAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns((PolicyAssignment?)null);

        // Act
        var response = await command.ExecuteAsync(context, args);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.Status);
        Assert.Contains("not found", response.Message.ToLower());
    }

    [Fact]
    public void GetCommand_ReturnsCorrectly()
    {
        // Arrange
        var command = new PolicyAssignmentGetCommand(_logger);

        // Act
        var cmdInstance = command.GetCommand();

        // Assert
        Assert.NotNull(cmdInstance);
        Assert.Equal("get", cmdInstance.Name);
    }
}
