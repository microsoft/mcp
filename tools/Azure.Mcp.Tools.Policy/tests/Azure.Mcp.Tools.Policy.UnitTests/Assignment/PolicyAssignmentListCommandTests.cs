// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.Policy.Commands;
using Azure.Mcp.Tools.Policy.Commands.Assignment;
using Azure.Mcp.Tools.Policy.Models;
using Azure.Mcp.Tools.Policy.Services;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Tests.Client;
using NSubstitute.ExceptionExtensions;

namespace Azure.Mcp.Tools.Policy.UnitTests.Assignment;

public class PolicyAssignmentListCommandTests : CommandUnitTestsBase<PolicyAssignmentListCommand, IPolicyService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("list", Command.Name);
        Assert.Equal("List Policy Assignments", Command.Title);
        Assert.Contains("policy assignment", Command.Description.ToLower());
        Assert.False(Command.Metadata.Destructive);
        Assert.True(Command.Metadata.Idempotent);
        Assert.False(Command.Metadata.OpenWorld);
        Assert.True(Command.Metadata.ReadOnly);
        Assert.False(Command.Metadata.LocalRequired);
        Assert.False(Command.Metadata.Secret);
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
        var args = new List<string>();

        if (!string.IsNullOrEmpty(subscription))
            args.AddRange(["--subscription", subscription]);
        if (!string.IsNullOrEmpty(scope))
            args.AddRange(["--scope", scope]);

        if (shouldSucceed)
        {
            Service.ListPolicyAssignmentsAsync(
                Arg.Any<string>(),
                Arg.Any<string?>(),
                Arg.Any<string?>(),
                Arg.Any<RetryPolicyOptions?>(),
                Arg.Any<CancellationToken>())
                .Returns([]);
        }

        // Act
        var response = await ExecuteCommandAsync(args.ToArray());

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

        Service.ListPolicyAssignmentsAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(assignments);

        // Act
        var response = await ExecuteCommandAsync("--subscription", "test-sub");

        // Assert
        var deserialized = ValidateAndDeserializeResponse(response, PolicyJsonContext.Default.PolicyAssignmentListCommandResult);

        Assert.Single(deserialized.Assignments);
        Assert.Equal("test-assignment", deserialized.Assignments[0].Name);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        // Arrange
        Service.ListPolicyAssignmentsAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Service error"));

        // Act
        var response = await ExecuteCommandAsync("--subscription", "test-sub");

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.NotNull(response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_WithScope_PassesScopeToService()
    {
        // Arrange
        Service.ListPolicyAssignmentsAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns([]);

        var scope = "/subscriptions/test-sub/resourceGroups/test-rg";

        // Act
        await ExecuteCommandAsync("--subscription", "test-sub", "--scope", scope);

        // Assert
        await Service.Received(1).ListPolicyAssignmentsAsync(
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
        Service.ListPolicyAssignmentsAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns([]);

        // Act
        await ExecuteCommandAsync("--subscription", "test-sub");

        // Assert
        await Service.Received(1).ListPolicyAssignmentsAsync(
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
        Service.ListPolicyAssignmentsAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns([]);

        // Act
        var response = await ExecuteCommandAsync("--subscription test-sub");

        // Assert
        var deserialized = ValidateAndDeserializeResponse(response, PolicyJsonContext.Default.PolicyAssignmentListCommandResult);
        Assert.Empty(deserialized.Assignments);
    }

    [Fact]
    public void GetCommand_ReturnsValidCommand()
    {
        Assert.Equal("list", CommandDefinition.Name);
        Assert.NotNull(CommandDefinition.Description);
    }
}
