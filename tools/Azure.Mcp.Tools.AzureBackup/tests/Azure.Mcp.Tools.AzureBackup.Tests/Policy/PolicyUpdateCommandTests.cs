// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tests.Commands;
using Azure.Mcp.Tools.AzureBackup.Commands;
using Azure.Mcp.Tools.AzureBackup.Commands.Policy;
using Azure.Mcp.Tools.AzureBackup.Models;
using Azure.Mcp.Tools.AzureBackup.Services;
using Microsoft.Mcp.Core.Options;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.AzureBackup.Tests.Policy;

public class PolicyUpdateCommandTests : SubscriptionCommandUnitTestsBase<PolicyUpdateCommand, IAzureBackupService>
{

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = Command.GetCommand();
        Assert.Equal("update", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Fact]
    public async Task ExecuteAsync_UpdatesPolicy_Successfully()
    {
        // Arrange
        var expected = new OperationResult("Succeeded", null, "Policy updated successfully");

        Service.UpdatePolicyAsync(
            Arg.Is("v"), Arg.Is("rg"), Arg.Is("sub"), Arg.Is("myPolicy"),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(),
            Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(expected);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--vault", "v",
            "--resource-group", "rg",
            "--policy", "myPolicy",
            "--schedule-time", "04:00",
            "--daily-retention-days", "60");

        // Assert
        var result = ValidateAndDeserializeResponse(response, AzureBackupJsonContext.Default.PolicyUpdateCommandResult);

        Assert.Equal("Succeeded", result.Result.Status);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesException()
    {
        // Arrange
        Service.UpdatePolicyAsync(
            Arg.Is("v"), Arg.Is("rg"), Arg.Is("sub"), Arg.Is("p"),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(),
            Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--vault", "v",
            "--resource-group", "rg",
            "--policy", "p");

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test error", response.Message);
    }

    [Theory]
    [InlineData("--subscription sub --vault v --resource-group rg --policy p", true)]
    [InlineData("--subscription sub --vault v --resource-group rg", false)] // Missing policy
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            Service.UpdatePolicyAsync(
                Arg.Is("v"), Arg.Is("rg"), Arg.Is("sub"), Arg.Is("p"),
                Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(),
                Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
                .Returns(new OperationResult("Succeeded", null, null));
        }

        // Act
        var response = await ExecuteCommandAsync(args);

        // Assert
        if (shouldSucceed)
        {
            Assert.Equal(HttpStatusCode.OK, response.Status);
        }
        else
        {
            Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        }
    }

    [Fact]
    public void BindOptions_BindsOptionsCorrectly()
    {
        // Arrange & Act
        var command = Command.GetCommand();
        var options = command.Options;

        // Assert
        Assert.Contains(options, o => o.Name == "--subscription");
        Assert.Contains(options, o => o.Name == "--resource-group");
        Assert.Contains(options, o => o.Name == "--vault");
        Assert.Contains(options, o => o.Name == "--vault-type");
        Assert.Contains(options, o => o.Name == "--policy");
        Assert.Contains(options, o => o.Name == "--schedule-time");
        Assert.Contains(options, o => o.Name == "--daily-retention-days");
    }

    [Fact]
    public void BindOptions_DoesNotContainWorkloadType()
    {
        var command = Command.GetCommand();
        var optionNames = command.Options.Select(o => o.Name).ToList();

        // Update command does not need workload-type since it reads from the existing policy
        Assert.DoesNotContain("--workload-type", optionNames);
    }

    [Fact]
    public async Task ExecuteAsync_DeserializationValidation()
    {
        // Arrange
        var expected = new OperationResult("Succeeded", null, "Policy updated successfully");

        Service.UpdatePolicyAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(),
            Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(expected);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--vault", "v",
            "--resource-group", "rg",
            "--policy", "p",
            "--daily-retention-days", "45");

        // Assert
        var result = ValidateAndDeserializeResponse(response, AzureBackupJsonContext.Default.PolicyUpdateCommandResult);

        Assert.Equal("Succeeded", result.Result.Status);
        Assert.Equal("Policy updated successfully", result.Result.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesAuthorizationFailure()
    {
        // Arrange
        Service.UpdatePolicyAsync(
            Arg.Is("v"), Arg.Is("rg"), Arg.Is("sub"), Arg.Is("p"),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(),
            Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException(403, "Forbidden"));

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--vault", "v",
            "--resource-group", "rg",
            "--policy", "p");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.Status);
        Assert.Contains("Authorization failed", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesNotFoundError()
    {
        // Arrange
        Service.UpdatePolicyAsync(
            Arg.Is("v"), Arg.Is("rg"), Arg.Is("sub"), Arg.Is("p"),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(),
            Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException(404, "Not Found"));

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--vault", "v",
            "--resource-group", "rg",
            "--policy", "p");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.Status);
        Assert.Contains("not found", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesDppNotSupportedError()
    {
        // Arrange
        Service.UpdatePolicyAsync(
            Arg.Is("v"), Arg.Is("rg"), Arg.Is("sub"), Arg.Is("p"),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(),
            Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new ArgumentException("Update is only supported for RSV (Recovery Services vault) policies. DPP policies do not support update."));

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--vault", "v",
            "--resource-group", "rg",
            "--policy", "p");

        // Assert — DPP-not-supported is a user-input error, returns BadRequest
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("RSV", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesUnsupportedPolicyTypeError()
    {
        // Arrange — an unsupported policy type is an input error, returns ArgumentException
        Service.UpdatePolicyAsync(
            Arg.Is("v"), Arg.Is("rg"), Arg.Is("sub"), Arg.Is("p"),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(),
            Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new ArgumentException("Unsupported policy type 'SomePolicy'."));

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--vault", "v",
            "--resource-group", "rg",
            "--policy", "p");

        // Assert — unsupported policy type is a user-input error, returns BadRequest
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("Unsupported policy type", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesInvalidScheduleTime()
    {
        // Arrange — invalid schedule time should return ArgumentException error
        Service.UpdatePolicyAsync(
            Arg.Is("v"), Arg.Is("rg"), Arg.Is("sub"), Arg.Is("p"),
            Arg.Any<string?>(), Arg.Is("not-a-time"), Arg.Any<string?>(),
            Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new ArgumentException("Invalid schedule time 'not-a-time'. Provide a valid time in UTC HH:mm format (e.g., '04:00')."));

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--vault", "v",
            "--resource-group", "rg",
            "--policy", "p",
            "--schedule-time", "not-a-time");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("Invalid schedule time", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesInvalidRetentionDays()
    {
        // Arrange — invalid retention days should return ArgumentException error
        Service.UpdatePolicyAsync(
            Arg.Is("v"), Arg.Is("rg"), Arg.Is("sub"), Arg.Is("p"),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Is("-5"),
            Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new ArgumentException("Invalid daily retention days '-5'. Provide a positive integer."));

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--vault", "v",
            "--resource-group", "rg",
            "--policy", "p",
            "--daily-retention-days", "-5");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("Invalid daily retention days", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_UpdatesWithScheduleTimeOnly()
    {
        // Arrange
        var expected = new OperationResult("Succeeded", null, "Policy 'p' updated in vault 'v'.");

        Service.UpdatePolicyAsync(
            Arg.Is("v"), Arg.Is("rg"), Arg.Is("sub"), Arg.Is("p"),
            Arg.Any<string?>(), Arg.Is("04:00"), Arg.Any<string?>(),
            Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(expected);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--vault", "v",
            "--resource-group", "rg",
            "--policy", "p",
            "--schedule-time", "04:00");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_UpdatesWithRetentionDaysOnly()
    {
        // Arrange
        var expected = new OperationResult("Succeeded", null, "Policy 'p' updated in vault 'v'.");

        Service.UpdatePolicyAsync(
            Arg.Is("v"), Arg.Is("rg"), Arg.Is("sub"), Arg.Is("p"),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Is("60"),
            Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(expected);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--vault", "v",
            "--resource-group", "rg",
            "--policy", "p",
            "--daily-retention-days", "60");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
    }
}
