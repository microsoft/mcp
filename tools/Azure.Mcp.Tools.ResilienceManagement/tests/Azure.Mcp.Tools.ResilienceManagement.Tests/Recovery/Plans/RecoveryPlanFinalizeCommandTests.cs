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

public sealed class RecoveryPlanFinalizeCommandTests : CommandUnitTestsBase<RecoveryPlanFinalizeCommand, IResilienceManagementService>
{
    [Fact]
    public void Constructor_DescribesPlanFinalizeSemantics()
    {
        var command = Command.GetCommand();

        Assert.Contains("validating resource permissions", command.Description, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("does not commit", command.Description, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_ForwardsRequestAndReturnsOperationId()
    {
        var expected = new RecoveryPlanFinalizeResult("11111111-1111-1111-1111-111111111111");
        Service.FinalizeRecoveryPlanAsync("sg1", "plan1", null, null, Arg.Any<CancellationToken>()).Returns(expected);

        var response = await ExecuteCommandAsync("--service-group", "sg1", "--recoveryplan", "plan1");

        RecoveryPlanFinalizeResult result = ValidateAndDeserializeResponse(response, ResilienceManagementJsonContext.Default.RecoveryPlanFinalizeResult);
        Assert.Equal(expected, result);
    }
}
