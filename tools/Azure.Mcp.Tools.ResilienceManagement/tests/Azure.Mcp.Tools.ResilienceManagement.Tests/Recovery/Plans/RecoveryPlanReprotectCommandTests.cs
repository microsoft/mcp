// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.ResilienceManagement.Commands;
using Azure.Mcp.Tools.ResilienceManagement.Commands.Recovery.Plans;
using Azure.Mcp.Tools.ResilienceManagement.Models;
using Azure.Mcp.Tools.ResilienceManagement.Services;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.ResilienceManagement.Tests.Recovery.Plans;

public sealed class RecoveryPlanReprotectCommandTests : CommandUnitTestsBase<RecoveryPlanReprotectCommand, IResilienceManagementService>
{
    private const string RecoveryResourceId = "/providers/Microsoft.Management/serviceGroups/sg1/providers/Microsoft.AzureResilienceManagement/recoveryPlans/plan1/recoveryResources/12345678-9012-3456-7890-123456789012";

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = Command.GetCommand();

        Assert.Equal("reprotect", command.Name);
        Assert.Contains("destructive", command.Description, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("operation ID", command.Description, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("validation-only", command.Description, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_RejectsMissingSelectedResources()
    {
        var response = await ExecuteCommandAsync(
            "--service-group", "sg1",
            "--recovery-plan", "plan1");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("selected-resource-ids", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_RejectsRecoveryResourceFromAnotherPlan()
    {
        const string otherPlanResourceId = "/providers/Microsoft.Management/serviceGroups/sg1/providers/Microsoft.AzureResilienceManagement/recoveryPlans/other-plan/recoveryResources/12345678-9012-3456-7890-123456789012";

        var response = await ExecuteCommandAsync(
            "--service-group", "sg1",
            "--recovery-plan", "plan1",
            "--selected-resource-ids", otherPlanResourceId);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("under the requested service group and recovery plan", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_ForwardsRequestAndReturnsOperationId()
    {
        var expected = new RecoveryPlanReprotectResult("11111111-1111-1111-1111-111111111111");
        Service.ReprotectRecoveryPlanAsync(
            "sg1",
            "plan1",
            Arg.Is<IReadOnlyList<string>>(resourceIds => resourceIds.SequenceEqual(new[] { RecoveryResourceId })),
            null,
            null,
            Arg.Any<CancellationToken>())
            .Returns(expected);

        var response = await ExecuteCommandAsync(
            "--service-group", "sg1",
            "--recovery-plan", "plan1",
            "--selected-resource-ids", RecoveryResourceId);

        RecoveryPlanReprotectResult result = ValidateAndDeserializeResponse(
            response,
            ResilienceManagementJsonContext.Default.RecoveryPlanReprotectResult);
        Assert.Equal(expected.OperationId, result.OperationId);
    }

    [Theory]
    [InlineData(HttpStatusCode.Conflict, "current state")]
    [InlineData(HttpStatusCode.Forbidden, "Authorization failed")]
    [InlineData(HttpStatusCode.NotFound, "not found")]
    [InlineData(HttpStatusCode.BadRequest, "request failed")]
    public async Task ExecuteAsync_SanitizesRequestFailedException(HttpStatusCode status, string expectedMessage)
    {
        const string providerDetails = "Sensitive provider details: request-id=123; endpoint=https://example.invalid";
        Service.ReprotectRecoveryPlanAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<IReadOnlyList<string>>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException((int)status, providerDetails));

        var response = await ExecuteCommandAsync(
            "--service-group", "sg1",
            "--recovery-plan", "plan1",
            "--selected-resource-ids", RecoveryResourceId);

        Assert.Equal(status, response.Status);
        Assert.Contains(expectedMessage, response.Message, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(providerDetails, response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_MapsTimeoutExceptionToGatewayTimeout()
    {
        const string internalDetails = "Internal request timeout details";
        Service.ReprotectRecoveryPlanAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<IReadOnlyList<string>>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new TimeoutException(internalDetails));

        var response = await ExecuteCommandAsync(
            "--service-group", "sg1",
            "--recovery-plan", "plan1",
            "--selected-resource-ids", RecoveryResourceId);

        Assert.Equal(HttpStatusCode.GatewayTimeout, response.Status);
        Assert.Contains("timed out", response.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("avoid starting the operation twice", response.Message, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(internalDetails, response.Message);
    }
}