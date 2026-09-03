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

public sealed class RecoveryJobResumeCommandTests : CommandUnitTestsBase<RecoveryJobResumeCommand, IResilienceManagementService>
{
    private const string RecoveryJob = "22222222-2222-2222-2222-222222222222";

    [Fact]
    public async Task ExecuteAsync_RejectsDescriptionLongerThanServiceContract()
    {
        var response = await ExecuteCommandAsync(
            "--service-group", "sg1",
            "--recoveryplan", "plan1",
            "--recoveryjob", RecoveryJob,
            "--description", new string('a', 101));

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("100 characters", response.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ExecuteAsync_ForwardsDescriptionAndReturnsOperationId()
    {
        var expected = new RecoveryJobResumeResult(
            "11111111-1111-1111-1111-111111111111",
            "Accepted",
            "Recovery job resume was accepted.");
        Service.ResumeRecoveryJobAsync("sg1", "plan1", RecoveryJob, "Approved", null, Arg.Any<CancellationToken>()).Returns(expected);

        var response = await ExecuteCommandAsync(
            "--service-group", "sg1",
            "--recoveryplan", "plan1",
            "--recoveryjob", RecoveryJob,
            "--description", "Approved");

        RecoveryJobResumeResult result = ValidateAndDeserializeResponse(response, ResilienceManagementJsonContext.Default.RecoveryJobResumeResult);
        Assert.Equal(expected, result);
    }

    [Fact]
    public async Task ExecuteAsync_ExplainsPausedStatePrecondition()
    {
        Service.ResumeRecoveryJobAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(), null, Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException((int)HttpStatusCode.PreconditionFailed, "provider details"));

        var response = await ExecuteCommandAsync("--service-group", "sg1", "--recoveryplan", "plan1", "--recoveryjob", RecoveryJob);

        Assert.Equal(HttpStatusCode.PreconditionFailed, response.Status);
        Assert.Contains("Paused state", response.Message, StringComparison.Ordinal);
        Assert.DoesNotContain("provider details", response.Message, StringComparison.Ordinal);
    }
}
