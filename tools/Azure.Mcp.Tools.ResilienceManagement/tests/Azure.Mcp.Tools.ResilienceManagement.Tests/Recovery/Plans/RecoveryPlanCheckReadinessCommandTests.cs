// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.ResilienceManagement.Commands;
using Azure.Mcp.Tools.ResilienceManagement.Commands.Recovery.Plans;
using Azure.Mcp.Tools.ResilienceManagement.Models;
using Azure.Mcp.Tools.ResilienceManagement.Services;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.ResilienceManagement.Tests.Recovery.Plans;

public sealed class RecoveryPlanCheckReadinessCommandTests : CommandUnitTestsBase<RecoveryPlanCheckReadinessCommand, IResilienceManagementService>
{
    private const string ValidArgs = "--service-group sg1 --recoveryplan plan1";

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = Command.GetCommand();
        Assert.Equal("checkreadiness", command.Name);
        Assert.Contains("protected resources", command.Description, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData(ValidArgs, true)]
    [InlineData("--service-group sg1", false)]
    [InlineData("--recoveryplan plan1", false)]
    [InlineData("", false)]
    public async Task ExecuteAsync_ValidatesRequiredInput(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            Service.CheckRecoveryPlanReadinessAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string?>(),
                Arg.Any<CancellationToken>())
                .Returns(CreateSuccessfulResult());
        }

        var response = await ExecuteCommandAsync(args);

        Assert.Equal(shouldSucceed ? HttpStatusCode.OK : HttpStatusCode.BadRequest, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_RejectsInvalidRecoveryPlanName()
    {
        var response = await ExecuteCommandAsync(
            "--service-group", "sg1",
            "--recoveryplan", "invalid_plan");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("5 to 24 characters", response.Message, StringComparison.OrdinalIgnoreCase);
        await Service.DidNotReceive().CheckRecoveryPlanReadinessAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_RejectsInvalidServiceGroupName()
    {
        var response = await ExecuteCommandAsync(
            "--service-group", "../sg1",
            "--recoveryplan", "plan1");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("service group name", response.Message, StringComparison.OrdinalIgnoreCase);
        await Service.DidNotReceive().CheckRecoveryPlanReadinessAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsCompletedReadinessResult()
    {
        Service.CheckRecoveryPlanReadinessAsync(
            "sg1",
            "plan1",
            null,
            Arg.Any<CancellationToken>())
            .Returns(CreateSuccessfulResult());

        var response = await ExecuteCommandAsync(ValidArgs);

        var result = ValidateAndDeserializeResponse(response, ResilienceManagementJsonContext.Default.RecoveryPlanReadinessResult);
        Assert.Equal("operation-1", result.OperationId);
        Assert.Equal("job-1", result.RecoveryJobId);
        Assert.True(result.IsReady);
        Assert.Equal("Completed", result.Status);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsReadinessFailures()
    {
        var error = new RecoveryPlanReadinessError("NotReady", "Resource requires attention.", ["Include or exclude the resource."]);
        Service.CheckRecoveryPlanReadinessAsync(
            "sg1",
            "plan1",
            null,
            Arg.Any<CancellationToken>())
            .Returns(new RecoveryPlanReadinessResult(
                "operation-1",
                "job-1",
                false,
                "Failed",
                error,
                [new("task-1", "ApplicationModificationTask", "Failed", error)],
                [new("target-1", "/subscriptions/sub1/resourceGroups/rg1/providers/Microsoft.Compute/disks/disk1", "Failed", "ApplicationModificationTask", error)]));

        var response = await ExecuteCommandAsync(ValidArgs);

        var result = ValidateAndDeserializeResponse(response, ResilienceManagementJsonContext.Default.RecoveryPlanReadinessResult);
        Assert.False(result.IsReady);
        Assert.Equal("Failed", result.Status);
        RecoveryPlanReadinessFailedTask failedTask = Assert.Single(result.FailedTasks);
        Assert.Equal("ApplicationModificationTask", failedTask.TaskName);
        Assert.Equal("NotReady", failedTask.Error?.Code);
        RecoveryPlanReadinessFailedResource failedResource = Assert.Single(result.FailedResources);
        Assert.EndsWith("/disks/disk1", failedResource.ResourceId, StringComparison.Ordinal);
        Assert.Equal("ApplicationModificationTask", failedResource.TaskName);
        Assert.Equal("NotReady", failedResource.Error?.Code);
        Assert.Equal("NotReady", result.Error?.Code);
    }

    [Theory]
    [InlineData(HttpStatusCode.Conflict, "current state")]
    [InlineData(HttpStatusCode.Forbidden, "Authorization failed")]
    [InlineData(HttpStatusCode.NotFound, "not found")]
    [InlineData(HttpStatusCode.BadRequest, "request failed")]
    public async Task ExecuteAsync_SanitizesRequestFailedException(HttpStatusCode status, string expectedMessage)
    {
        const string providerDetails = "Sensitive provider details: request-id=123; endpoint=https://example.invalid";
        Service.CheckRecoveryPlanReadinessAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException((int)status, providerDetails));

        var response = await ExecuteCommandAsync(ValidArgs);

        Assert.Equal(status, response.Status);
        Assert.Contains(expectedMessage, response.Message, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(providerDetails, response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_MapsTimeoutExceptionToGatewayTimeout()
    {
        const string internalDetails = "Internal polling timeout details";
        Service.CheckRecoveryPlanReadinessAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new TimeoutException(internalDetails));

        var response = await ExecuteCommandAsync(ValidArgs);

        Assert.Equal(HttpStatusCode.GatewayTimeout, response.Status);
        Assert.Contains("timed out", response.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Retry", response.Message, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(internalDetails, response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_MapsMissingJobIdToBadGateway()
    {
        const string internalDetails = "Internal response parsing details";
        Service.CheckRecoveryPlanReadinessAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException(internalDetails));

        var response = await ExecuteCommandAsync(ValidArgs);

        Assert.Equal(HttpStatusCode.BadGateway, response.Status);
        Assert.Contains("without returning a valid recovery job response", response.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Retry", response.Message, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(internalDetails, response.Message);
    }

    private static RecoveryPlanReadinessResult CreateSuccessfulResult() => new(
        "operation-1",
        "job-1",
        true,
        "Completed",
        null,
        [],
        []);
}
