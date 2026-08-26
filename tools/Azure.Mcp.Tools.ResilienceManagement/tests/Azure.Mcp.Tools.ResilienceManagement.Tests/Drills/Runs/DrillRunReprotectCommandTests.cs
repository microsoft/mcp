// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.ResilienceManagement.Commands;
using Azure.Mcp.Tools.ResilienceManagement.Commands.Drills.Runs;
using Azure.Mcp.Tools.ResilienceManagement.Services;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.ResilienceManagement.Tests.Drills.Runs;

public sealed class DrillRunReprotectCommandTests : CommandUnitTestsBase<DrillRunReprotectCommand, IResilienceManagementService>
{
    private const string ServiceGroup = "sg1";
    private const string Drill = "drill1";
    private const string DrillRun = "run1";

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = Command.GetCommand();
        Assert.Equal("reprotect", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Theory]
    [InlineData()]
    [InlineData("--service-group", ServiceGroup)]
    [InlineData("--service-group", ServiceGroup, "--drill", Drill)]
    public async Task ExecuteAsync_RejectsMissingRequiredOptions(params string[] args)
    {
        var response = await ExecuteCommandAsync(args);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
    }

    [Theory]
    [InlineData("../sg1", Drill, DrillRun)]
    [InlineData(ServiceGroup, "drill/one", DrillRun)]
    [InlineData(ServiceGroup, Drill, "run\\one")]
    public async Task ExecuteAsync_RejectsInvalidResourceNames(string serviceGroup, string drill, string drillRun)
    {
        var response = await ExecuteCommandAsync(
            "--service-group", serviceGroup,
            "--drill", drill,
            "--drill-run", drillRun);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        await Service.DidNotReceive().ReprotectDrillRunAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_ReprotectsDrillRunAndReturnsAcceptedResult()
    {
        var response = await ExecuteCommandAsync(
            "--service-group", ServiceGroup,
            "--drill", Drill,
            "--drill-run", DrillRun);

        var result = ValidateAndDeserializeResponse(response, ResilienceManagementJsonContext.Default.DrillRunReprotectCommandResult);
        Assert.True(result.Accepted);
        Assert.Equal(DrillRun, result.DrillRun);
        await Service.Received(1).ReprotectDrillRunAsync(
            ServiceGroup,
            Drill,
            DrillRun,
            null,
            null,
            Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(HttpStatusCode.Conflict, "current state")]
    [InlineData(HttpStatusCode.Forbidden, "Authorization failed")]
    [InlineData(HttpStatusCode.NotFound, "not found")]
    [InlineData(HttpStatusCode.BadRequest, "request failed")]
    public async Task ExecuteAsync_SanitizesRequestFailedException(HttpStatusCode status, string expectedMessage)
    {
        const string providerDetails = "Sensitive provider details: request-id=123; endpoint=https://example.invalid";
        Service.ReprotectDrillRunAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException((int)status, providerDetails));

        var response = await ExecuteCommandAsync(
            "--service-group", ServiceGroup,
            "--drill", Drill,
            "--drill-run", DrillRun);

        Assert.Equal(status, response.Status);
        Assert.Contains(expectedMessage, response.Message, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(providerDetails, response.Message);
    }
}
