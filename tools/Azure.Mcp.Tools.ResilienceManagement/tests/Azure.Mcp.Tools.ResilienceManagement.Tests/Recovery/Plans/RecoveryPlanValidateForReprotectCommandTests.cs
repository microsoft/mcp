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

public sealed class RecoveryPlanValidateForReprotectCommandTests : CommandUnitTestsBase<RecoveryPlanValidateForReprotectCommand, IResilienceManagementService>
{
    private const string RecoveryResourceId = "/providers/Microsoft.Management/serviceGroups/sg1/providers/Microsoft.AzureResilienceManagement/recoveryPlans/plan1/recoveryResources/12345678-9012-3456-7890-123456789012";

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = Command.GetCommand();

        Assert.Equal("validateforreprotect", command.Name);
        Assert.Contains("reprotect", command.Description, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("blocking reasons", command.Description, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_WithoutSelectedResources_ValidatesAllQualifiedResources()
    {
        var expected = new RecoveryPlanValidateForReprotectResult(
            "11111111-1111-1111-1111-111111111111",
            []);
        Service.ValidateRecoveryPlanForReprotectAsync(
            "sg1",
            "plan1",
            null,
            null,
            Arg.Any<CancellationToken>())
            .Returns(expected);

        var response = await ExecuteCommandAsync(
            "--service-group", "sg1",
            "--recoveryplan", "plan1");

        RecoveryPlanValidateForReprotectResult result = ValidateAndDeserializeResponse(
            response,
            ResilienceManagementJsonContext.Default.RecoveryPlanValidateForReprotectResult);
        Assert.Equal(expected.OperationId, result.OperationId);
        Assert.Empty(result.RecoveryResourceQualifications);
    }

    [Fact]
    public async Task ExecuteAsync_WithSelectedResources_ForwardsRequestAndReturnsQualifications()
    {
        var expected = new RecoveryPlanValidateForReprotectResult(
            "11111111-1111-1111-1111-111111111111",
            [new RecoveryPlanFailoverQualification(
                RecoveryResourceId,
                "12345678-9012-3456-7890-123456789012",
                "/subscriptions/00000000-0000-0000-0000-000000000000/resourceGroups/rg/providers/Microsoft.Compute/virtualMachines/vm1",
                "eastus",
                "NotQualified",
                ["ReprotectOperationNotAllowedInCurrentProtectionState"],
                ["eastus-az1"],
                "Included",
                "Protected",
                false,
                [])]);
        Service.ValidateRecoveryPlanForReprotectAsync(
            "sg1",
            "plan1",
            Arg.Is<IReadOnlyList<string>>(resourceIds => resourceIds.SequenceEqual(new[] { RecoveryResourceId })),
            null,
            Arg.Any<CancellationToken>())
            .Returns(expected);

        var response = await ExecuteCommandAsync(
            "--service-group", "sg1",
            "--recoveryplan", "plan1",
            "--selected-resource-ids", RecoveryResourceId);

        RecoveryPlanValidateForReprotectResult result = ValidateAndDeserializeResponse(
            response,
            ResilienceManagementJsonContext.Default.RecoveryPlanValidateForReprotectResult);
        RecoveryPlanFailoverQualification qualification = Assert.Single(result.RecoveryResourceQualifications);
        Assert.Equal("NotQualified", qualification.QualificationState);
        Assert.Equal(["ReprotectOperationNotAllowedInCurrentProtectionState"], qualification.NotQualifiedReasons);
    }

    [Fact]
    public async Task ExecuteAsync_RejectsRecoveryResourceFromAnotherPlan()
    {
        const string otherPlanResourceId = "/providers/Microsoft.Management/serviceGroups/sg1/providers/Microsoft.AzureResilienceManagement/recoveryPlans/other-plan/recoveryResources/12345678-9012-3456-7890-123456789012";

        var response = await ExecuteCommandAsync(
            "--service-group", "sg1",
            "--recoveryplan", "plan1",
            "--selected-resource-ids", otherPlanResourceId);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("under the requested service group and recoveryplan", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData(HttpStatusCode.Forbidden, "Authorization failed")]
    [InlineData(HttpStatusCode.NotFound, "not found")]
    [InlineData(HttpStatusCode.BadRequest, "request failed")]
    public async Task ExecuteAsync_SanitizesRequestFailedException(HttpStatusCode status, string expectedMessage)
    {
        const string providerDetails = "Sensitive provider details: request-id=123; endpoint=https://example.invalid";
        Service.ValidateRecoveryPlanForReprotectAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<IReadOnlyList<string>?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException((int)status, providerDetails));

        var response = await ExecuteCommandAsync(
            "--service-group", "sg1",
            "--recoveryplan", "plan1");

        Assert.Equal(status, response.Status);
        Assert.Contains(expectedMessage, response.Message, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(providerDetails, response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_MapsTimeoutExceptionToGatewayTimeout()
    {
        const string internalDetails = "Internal polling timeout details";
        Service.ValidateRecoveryPlanForReprotectAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<IReadOnlyList<string>?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new TimeoutException(internalDetails));

        var response = await ExecuteCommandAsync(
            "--service-group", "sg1",
            "--recoveryplan", "plan1");

        Assert.Equal(HttpStatusCode.GatewayTimeout, response.Status);
        Assert.Contains("timed out", response.Message, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(internalDetails, response.Message);
    }
}
