// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.ResilienceManagement.Commands;
using Azure.Mcp.Tools.ResilienceManagement.Commands.Recovery.Jobs;
using Azure.Mcp.Tools.ResilienceManagement.Models;
using Azure.Mcp.Tools.ResilienceManagement.Services;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.ResilienceManagement.Tests.Recovery.Jobs;

public sealed class RecoveryJobRetryCommandTests : CommandUnitTestsBase<RecoveryJobRetryCommand, IResilienceManagementService>
{
    private const string RecoveryJob = "22222222-2222-2222-2222-222222222222";

    [Fact]
    public async Task ExecuteAsync_RejectsRecoveryJobThatIsNotAGuid()
    {
        var response = await ExecuteCommandAsync("--service-group", "sg1", "--recoveryplan", "plan1", "--recovery-job", "not-a-guid");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("GUID in D format", response.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ExecuteAsync_ForwardsFailedJobAndReturnsOperationId()
    {
        var expected = new RecoveryJobRetryResult("11111111-1111-1111-1111-111111111111");
        Service.RetryRecoveryJobAsync("sg1", "plan1", RecoveryJob, null, Arg.Any<CancellationToken>()).Returns(expected);

        var response = await ExecuteCommandAsync("--service-group", "sg1", "--recoveryplan", "plan1", "--recovery-job", RecoveryJob);

        RecoveryJobRetryResult result = ValidateAndDeserializeResponse(response, ResilienceManagementJsonContext.Default.RecoveryJobRetryResult);
        Assert.Equal(expected, result);
    }

    [Fact]
    public async Task ExecuteAsync_ExplainsFailedStatePrecondition()
    {
        Service.RetryRecoveryJobAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), null, Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException((int)HttpStatusCode.PreconditionFailed, "provider details"));

        var response = await ExecuteCommandAsync("--service-group", "sg1", "--recoveryplan", "plan1", "--recovery-job", RecoveryJob);

        Assert.Equal(HttpStatusCode.PreconditionFailed, response.Status);
        Assert.Contains("Failed state", response.Message, StringComparison.Ordinal);
        Assert.DoesNotContain("provider details", response.Message, StringComparison.Ordinal);
    }
}
