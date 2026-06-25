// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tests.Commands;
using Azure.Mcp.Tools.ResilienceManagement.Commands;
using Azure.Mcp.Tools.ResilienceManagement.Commands.Goals.Templates;
using Azure.Mcp.Tools.ResilienceManagement.Models;
using Azure.Mcp.Tools.ResilienceManagement.Services;
using Microsoft.Mcp.Core.Options;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.ResilienceManagement.Tests.Goals.Templates;

public sealed class GoalTemplateCreateCommandTests : SubscriptionCommandUnitTestsBase<GoalTemplateCreateCommand, IResilienceManagementService>
{
    private const string ValidArgs =
        "--service-group sg --goal-template gt1 --goal-type Resiliency " +
        "--require-high-availability Required --require-disaster-recovery NotRequired " +
        "--regional-recovery-point-objective PT15M --regional-recovery-time-objective PT30M --subscription sub";

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
    [InlineData("--goal-template gt1 --goal-type Resiliency --require-high-availability Required --require-disaster-recovery NotRequired --regional-recovery-point-objective PT15M --regional-recovery-time-objective PT30M --subscription sub", false)] // Missing service group
    [InlineData("--service-group sg --goal-type Resiliency --require-high-availability Required --require-disaster-recovery NotRequired --regional-recovery-point-objective PT15M --regional-recovery-time-objective PT30M --subscription sub", false)] // Missing goal template
    [InlineData("--service-group sg --goal-template gt1 --goal-type Resiliency --require-high-availability Required --require-disaster-recovery NotRequired --regional-recovery-point-objective PT15M --regional-recovery-time-objective PT30M", false)] // Missing subscription
    [InlineData("", false)] // No parameters
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        // Arrange
        if (shouldSucceed)
        {
            Service.CreateGoalTemplateAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<GoalTemplateKind>(),
                Arg.Any<GoalRequirement>(),
                Arg.Any<GoalRequirement>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string?>(),
                Arg.Any<RetryPolicyOptions?>(),
                Arg.Any<CancellationToken>())
                .Returns(new GoalTemplateInfo("id1", "gt1"));
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
    public async Task ExecuteAsync_ReturnsCreatedGoalTemplate()
    {
        // Arrange
        Service.CreateGoalTemplateAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<GoalTemplateKind>(),
            Arg.Any<GoalRequirement>(),
            Arg.Any<GoalRequirement>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(new GoalTemplateInfo("id1", "gt1"));

        // Act
        var response = await ExecuteCommandAsync(ValidArgs);

        // Assert
        var result = ValidateAndDeserializeResponse(response, ResilienceManagementJsonContext.Default.GoalTemplateCreateCommandResult);
        Assert.NotNull(result.GoalTemplate);
        Assert.Equal("gt1", result.GoalTemplate.Name);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        // Arrange
        Service.CreateGoalTemplateAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<GoalTemplateKind>(),
            Arg.Any<GoalRequirement>(),
            Arg.Any<GoalRequirement>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
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
