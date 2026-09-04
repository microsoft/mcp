// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.ResilienceManagement.Commands;
using Azure.Mcp.Tools.ResilienceManagement.Commands.Recovery.Plans;
using Azure.Mcp.Tools.ResilienceManagement.Models;
using Azure.Mcp.Tools.ResilienceManagement.Services;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.ResilienceManagement.Tests.Recovery.Plans;

public sealed class RecoveryPlanReprotectCommandTests : CommandUnitTestsBase<RecoveryPlanReprotectCommand, IResilienceManagementService>
{
    private const string RecoveryResourceId = "/providers/Microsoft.Management/serviceGroups/sg1/providers/Microsoft.AzureResilienceManagement/recoveryPlans/plan1/recoveryResources/12345678-9012-3456-7890-123456789012";

    [Fact]
    public async Task ExecuteAsync_AllowsAllQualifiedResourcesWhenSelectionOmitted()
    {
        var expected = new RecoveryPlanReprotectResult(
            "11111111-1111-1111-1111-111111111111",
            "22222222-2222-2222-2222-222222222222",
            "Accepted",
            "Reprotect was accepted.");
        Service.ReprotectRecoveryPlanAsync("sg1", "plan1", null, null, Arg.Any<CancellationToken>()).Returns(expected);

        var response = await ExecuteCommandAsync("--service-group", "sg1", "--recoveryplan", "plan1");

        RecoveryPlanReprotectResult result = ValidateAndDeserializeResponse(response, ResilienceManagementJsonContext.Default.RecoveryPlanReprotectResult);
        Assert.Equal(expected, result);
    }

    [Fact]
    public async Task ExecuteAsync_ForwardsSelectedResources()
    {
        var expected = new RecoveryPlanReprotectResult(
            "11111111-1111-1111-1111-111111111111",
            "22222222-2222-2222-2222-222222222222",
            "Accepted",
            "Reprotect was accepted.");
        Service.ReprotectRecoveryPlanAsync(
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

        RecoveryPlanReprotectResult result = ValidateAndDeserializeResponse(response, ResilienceManagementJsonContext.Default.RecoveryPlanReprotectResult);
        Assert.Equal(expected, result);
    }
}
