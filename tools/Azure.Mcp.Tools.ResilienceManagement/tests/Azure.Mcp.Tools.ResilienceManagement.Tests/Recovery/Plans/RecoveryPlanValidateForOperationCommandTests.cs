// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.ResilienceManagement.Commands;
using Azure.Mcp.Tools.ResilienceManagement.Commands.Recovery.Plans;
using Azure.Mcp.Tools.ResilienceManagement.Models;
using Azure.Mcp.Tools.ResilienceManagement.Services;
using Azure.ResourceManager.ResilienceManagement.Models;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.ResilienceManagement.Tests.Recovery.Plans;

public sealed class RecoveryPlanValidateForOperationCommandTests : CommandUnitTestsBase<RecoveryPlanValidateForOperationCommand, IResilienceManagementService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = Command.GetCommand();

        Assert.Equal("validateforoperation", command.Name);
        Assert.Contains("current state", command.Description, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("does not execute", command.Description, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("Failover")]
    [InlineData("FailoverCommit")]
    [InlineData("Reprotect")]
    [InlineData("TestFailover")]
    [InlineData("TestFailoverCleanup")]
    public async Task ExecuteAsync_WithSupportedOperation_ForwardsCanonicalOperation(string operationName)
    {
        var expected = new RecoveryPlanValidateForOperationResult(
            "11111111-1111-1111-1111-111111111111",
            operationName,
            true,
            null,
            null);
        Service.ValidateRecoveryPlanForOperationAsync(
            "sg1",
            "plan1",
            Arg.Is<RecoveryOperationNames>(value => value.ToString() == operationName),
            null,
            Arg.Any<CancellationToken>())
            .Returns(expected);

        var response = await ExecuteCommandAsync(
            "--service-group", "sg1",
            "--recoveryplan", "plan1",
            "--operation-name", operationName.ToLowerInvariant());

        RecoveryPlanValidateForOperationResult result = ValidateAndDeserializeResponse(
            response,
            ResilienceManagementJsonContext.Default.RecoveryPlanValidateForOperationResult);
        Assert.Equal(expected, result);
    }

    [Fact]
    public async Task ExecuteAsync_WithUnsupportedOperation_ReturnsBadRequest()
    {
        var response = await ExecuteCommandAsync(
            "--service-group", "sg1",
            "--recoveryplan", "plan1",
            "--operation-name", "Delete");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("must be Failover", response.Message, StringComparison.OrdinalIgnoreCase);
        await Service.DidNotReceiveWithAnyArgs().ValidateRecoveryPlanForOperationAsync(
            default!, default!, default, default, TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task ExecuteAsync_WhenOperationIsNotValid_ReturnsStructuredError()
    {
        var expected = new RecoveryPlanValidateForOperationResult(
            "11111111-1111-1111-1111-111111111111",
            "Reprotect",
            false,
            "RecoveryPlanStateDoesNotSupportOperation",
            "Operation Reprotect is not allowed for the current recoveryplan state.");
        Service.ValidateRecoveryPlanForOperationAsync(
            "sg1",
            "plan1",
            Arg.Is<RecoveryOperationNames>(value => value == RecoveryOperationNames.Reprotect),
            null,
            Arg.Any<CancellationToken>())
            .Returns(expected);

        var response = await ExecuteCommandAsync(
            "--service-group", "sg1",
            "--recoveryplan", "plan1",
            "--operation-name", "Reprotect");

        RecoveryPlanValidateForOperationResult result = ValidateAndDeserializeResponse(
            response,
            ResilienceManagementJsonContext.Default.RecoveryPlanValidateForOperationResult);
        Assert.False(result.IsValid);
        Assert.Equal(expected.ErrorCode, result.ErrorCode);
        Assert.Equal(expected.ErrorMessage, result.ErrorMessage);
    }

    [Theory]
    [InlineData(HttpStatusCode.Forbidden, "Authorization failed")]
    [InlineData(HttpStatusCode.NotFound, "not found")]
    [InlineData(HttpStatusCode.BadRequest, "request failed")]
    public async Task ExecuteAsync_SanitizesRequestFailedException(HttpStatusCode status, string expectedMessage)
    {
        const string providerDetails = "Sensitive provider details: request-id=123; endpoint=https://example.invalid";
        Service.ValidateRecoveryPlanForOperationAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RecoveryOperationNames>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException((int)status, providerDetails));

        var response = await ExecuteCommandAsync(
            "--service-group", "sg1",
            "--recoveryplan", "plan1",
            "--operation-name", "Failover");

        Assert.Equal(status, response.Status);
        Assert.Contains(expectedMessage, response.Message, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(providerDetails, response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_MapsTimeoutExceptionToGatewayTimeout()
    {
        const string internalDetails = "Internal polling timeout details";
        Service.ValidateRecoveryPlanForOperationAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RecoveryOperationNames>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new TimeoutException(internalDetails));

        var response = await ExecuteCommandAsync(
            "--service-group", "sg1",
            "--recoveryplan", "plan1",
            "--operation-name", "Failover");

        Assert.Equal(HttpStatusCode.GatewayTimeout, response.Status);
        Assert.Contains("timed out", response.Message, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(internalDetails, response.Message);
    }
}
