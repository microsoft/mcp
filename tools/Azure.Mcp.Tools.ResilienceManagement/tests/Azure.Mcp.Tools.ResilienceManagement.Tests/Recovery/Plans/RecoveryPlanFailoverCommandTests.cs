// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.ResilienceManagement.Commands;
using Azure.Mcp.Tools.ResilienceManagement.Commands.Recovery.Plans;
using Azure.Mcp.Tools.ResilienceManagement.Models;
using Azure.Mcp.Tools.ResilienceManagement.Services;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.ResilienceManagement.Tests.Recovery.Plans;

public sealed class RecoveryPlanFailoverCommandTests : CommandUnitTestsBase<RecoveryPlanFailoverCommand, IResilienceManagementService>
{
    private const string RecoveryResourceId = "/providers/Microsoft.Management/serviceGroups/sg1/providers/Microsoft.AzureResilienceManagement/recoveryPlans/plan1/recoveryResources/12345678-9012-3456-7890-123456789012";

    [Fact]
    public async Task ExecuteAsync_RejectsRequestWithoutResourceSelectors()
    {
        var response = await ExecuteCommandAsync("--service-group", "sg1", "--recoveryplan", "plan1");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("source-locations or --selected-resource-ids", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_RejectsUnsupportedUserConsent()
    {
        var response = await ExecuteCommandAsync(
            "--service-group", "sg1",
            "--recoveryplan", "plan1",
            "--source-locations", "eastus",
            "--user-consent", "Denied");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("Unspecified or Allowed", response.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("/providers/Contoso.Management/serviceGroups/sg1/providers/Microsoft.AzureResilienceManagement/recoveryPlans/plan1/recoveryResources/12345678-9012-3456-7890-123456789012")]
    [InlineData("/providers/Microsoft.Management/serviceGroups/sg1/providers/Contoso.AzureResilienceManagement/recoveryPlans/plan1/providers/Microsoft.AzureResilienceManagement/recoveryResources/12345678-9012-3456-7890-123456789012")]
    public async Task ExecuteAsync_RejectsSelectedResourceWithInvalidAncestorType(string recoveryResourceId)
    {
        var response = await ExecuteCommandAsync(
            "--service-group", "sg1",
            "--recoveryplan", "plan1",
            "--selected-resource-ids", recoveryResourceId);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("under the requested service group and recoveryplan", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_ForwardsRequestAndReturnsOperationAndJobIds()
    {
        var expected = new RecoveryPlanFailoverResult(
            "11111111-1111-1111-1111-111111111111",
            "22222222-2222-2222-2222-222222222222",
            "Accepted",
            "Failover was accepted.");
        Service.FailoverRecoveryPlanAsync(
            "sg1",
            "plan1",
            Arg.Is<IReadOnlyList<string>>(locations => locations.SequenceEqual(new[] { "eastus" })),
            Arg.Is<IReadOnlyList<string>>(resourceIds => resourceIds.SequenceEqual(new[] { RecoveryResourceId })),
            "Allowed",
            null,
            Arg.Any<CancellationToken>())
            .Returns(expected);

        var response = await ExecuteCommandAsync(
            "--service-group", "sg1",
            "--recoveryplan", "plan1",
            "--source-locations", "eastus",
            "--selected-resource-ids", RecoveryResourceId,
            "--user-consent", "Allowed");

        RecoveryPlanFailoverResult result = ValidateAndDeserializeResponse(response, ResilienceManagementJsonContext.Default.RecoveryPlanFailoverResult);
        Assert.Equal(expected, result);
    }
}
