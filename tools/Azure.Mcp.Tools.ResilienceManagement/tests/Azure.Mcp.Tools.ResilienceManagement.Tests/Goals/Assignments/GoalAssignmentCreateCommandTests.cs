// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tests.Commands;
using Azure.Mcp.Tools.ResilienceManagement.Commands;
using Azure.Mcp.Tools.ResilienceManagement.Commands.Goals.Assignments;
using Azure.Mcp.Tools.ResilienceManagement.Models;
using Azure.Mcp.Tools.ResilienceManagement.Services;
using Microsoft.Mcp.Core.Options;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.ResilienceManagement.Tests.Goals.Assignments;

public sealed class GoalAssignmentCreateCommandTests : SubscriptionCommandUnitTestsBase<GoalAssignmentCreateCommand, IResilienceManagementService>
{
    private const string ValidArgs =
        "--service-group sg --goal-assignment ga1 --goal-template gt1 --goal-assignment-type Resiliency --subscription sub";

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = Command.GetCommand();
        Assert.Equal("create", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Theory]
    [InlineData(ValidArgs, true)]
    [InlineData("--goal-assignment ga1 --goal-template gt1 --goal-assignment-type Resiliency --subscription sub", false)] // Missing service group
    [InlineData("--service-group sg --goal-template gt1 --goal-assignment-type Resiliency --subscription sub", false)] // Missing goal assignment
    [InlineData("--service-group sg --goal-assignment ga1 --goal-assignment-type Resiliency --subscription sub", false)] // Missing goal template
    [InlineData("--service-group sg --goal-assignment ga1 --goal-template gt1 --goal-assignment-type Resiliency", false)] // Missing subscription
    [InlineData("", false)] // No parameters
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        // Arrange
        if (shouldSucceed)
        {
            Service.CreateGoalAssignmentAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<GoalAssignmentKind>(),
                Arg.Any<string>(),
                Arg.Any<string?>(),
                Arg.Any<RetryPolicyOptions?>(),
                Arg.Any<CancellationToken>())
                .Returns(new GoalAssignmentInfo("id1", "ga1"));
        }

        // Act
        var response = await ExecuteCommandAsync(args);

        // Assert
        Assert.Equal(shouldSucceed ? HttpStatusCode.OK : HttpStatusCode.BadRequest, response.Status);
        if (!shouldSucceed)
        {
            Assert.Contains("required", response.Message.ToLower());
        }
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsCreatedGoalAssignment()
    {
        // Arrange
        Service.CreateGoalAssignmentAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<GoalAssignmentKind>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(new GoalAssignmentInfo("id1", "ga1"));

        // Act
        var response = await ExecuteCommandAsync(ValidArgs);

        // Assert
        var result = ValidateAndDeserializeResponse(response, ResilienceManagementJsonContext.Default.GoalAssignmentCreateCommandResult);
        Assert.NotNull(result.GoalAssignment);
        Assert.Equal("ga1", result.GoalAssignment.Name);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        // Arrange
        Service.CreateGoalAssignmentAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<GoalAssignmentKind>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        // Act
        var response = await ExecuteCommandAsync(ValidArgs);

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.StartsWith("Test error", response.Message);
    }
}
