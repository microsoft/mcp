// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.ResilienceManagement.Commands.Goals.Assignments;
using Azure.Mcp.Tools.ResilienceManagement.Services;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.ResilienceManagement.Tests.Goals;

public class GoalPollingRegressionTests : CommandUnitTestsBase<GoalAssignmentCreateCommand, IResilienceManagementService>
{
    [Fact]
    public async Task FailedOperationWithSuccessfulHttpStatus_IsNotSuccess()
    {
        Service.CreateGoalAssignmentAsync("sg1", "goal1", true,
            Arg.Any<IReadOnlyList<string>?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException(200, "provider-detail", "RHUBUserErrorServiceGroupMembersLimitExceeded", null));

        var response = await ExecuteCommandAsync("--service-group", "sg1", "--goal-assignment", "goal1", "--require-zonal-resiliency", "true");

        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("500-member", response.Message);
        Assert.DoesNotContain("provider-detail", response.Message);
        Assert.Null(response.Results);
    }

    [Theory]
    [InlineData("*C3775EF2AC0E1E4AA241CB87BEE5AA96EBB7B61E6391AE2CEABF0BAB8702AA1E", true)]
    [InlineData("%2AC3775EF2AC0E1E4AA241CB87BEE5AA96EBB7B61E6391AE2CEABF0BAB8702AA1E", true)]
    [InlineData("*invalid", false)]
    [InlineData("extra", false)]
    public void PollingUrl_ExtractsExactGuidBeforeRoutingHash(string suffix, bool valid)
    {
        const string id = "10b982d0-4c82-484a-942b-cf694cdcac58";
        string? result = ResilienceManagementService.ExtractGoalOperationId(
            $"https://management.azure.com/providers/Microsoft.AzureResilienceManagement/locations/CENTRALUS/operationStatuses/{id}{suffix}?api-version=2026-10-01");

        Assert.Equal(valid ? id : null, result);
    }
}
