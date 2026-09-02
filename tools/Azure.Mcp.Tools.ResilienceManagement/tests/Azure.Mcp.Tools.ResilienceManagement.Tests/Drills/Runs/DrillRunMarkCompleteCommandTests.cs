// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.ResilienceManagement.Commands;
using Azure.Mcp.Tools.ResilienceManagement.Commands.Drills.Runs;
using Azure.Mcp.Tools.ResilienceManagement.Models;
using Azure.Mcp.Tools.ResilienceManagement.Services;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.ResilienceManagement.Tests.Drills.Runs;

public sealed class DrillRunMarkCompleteCommandTests
    : CommandUnitTestsBase<DrillRunMarkCompleteCommand, IResilienceManagementService>
{
    private const string ServiceGroup = "sg1";
    private const string Drill = "drill1";
    private const string DrillRun = "run1";
    private const string Stage = "FaultInjection";

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = Command.GetCommand();

        Assert.Equal("mark-complete", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Theory]
    [InlineData("--drill drill1 --drill-run run1 --stage FaultInjection", false)]
    [InlineData("--service-group sg1 --drill-run run1 --stage FaultInjection", false)]
    [InlineData("--service-group sg1 --drill drill1 --stage FaultInjection", false)]
    [InlineData("--service-group sg1 --drill drill1 --drill-run run1", false)]
    public async Task ExecuteAsync_ValidatesRequiredInput(string args, bool shouldSucceed)
    {
        var response = await ExecuteCommandAsync(args);

        Assert.Equal(shouldSucceed ? HttpStatusCode.OK : HttpStatusCode.BadRequest, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_MarksStageCompleteAndReturnsOperation()
    {
        Service.MarkDrillRunCompleteAsync(
            ServiceGroup,
            Drill,
            DrillRun,
            Stage,
            "tenant1",
            Arg.Any<CancellationToken>())
            .Returns(new DrillRunMarkCompleteResult("operation1", false));

        var response = await ExecuteCommandAsync(
            "--service-group", ServiceGroup,
            "--drill", Drill,
            "--drill-run", DrillRun,
            "--stage", Stage,
            "--tenant", "tenant1");

        var result = ValidateAndDeserializeResponse(
            response,
            ResilienceManagementJsonContext.Default.DrillRunMarkCompleteCommandResult);
        Assert.Equal("operation1", result.Result.OperationId);
        Assert.False(result.Result.HasCompleted);
    }

    [Fact]
    public async Task ExecuteAsync_NormalizesStageCasing()
    {
        var response = await ExecuteCommandAsync(
            "--service-group", ServiceGroup,
            "--drill", Drill,
            "--drill-run", DrillRun,
            "--stage", "faultinjection");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).MarkDrillRunCompleteAsync(
            ServiceGroup,
            Drill,
            DrillRun,
            "FaultInjection",
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_RejectsUnknownStage()
    {
        var response = await ExecuteCommandAsync(
            "--service-group", ServiceGroup,
            "--drill", Drill,
            "--drill-run", DrillRun,
            "--stage", "NotAStage");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("--stage must be one of", response.Message);
        await Service.DidNotReceive().MarkDrillRunCompleteAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("--service-group", "sg/1")]
    [InlineData("--drill", "drill/1")]
    [InlineData("--drill-run", "run/1")]
    public async Task ExecuteAsync_RejectsInvalidPathSegments(string invalidOption, string invalidValue)
    {
        string serviceGroup = invalidOption == "--service-group" ? invalidValue : ServiceGroup;
        string drill = invalidOption == "--drill" ? invalidValue : Drill;
        string drillRun = invalidOption == "--drill-run" ? invalidValue : DrillRun;

        var response = await ExecuteCommandAsync(
            "--service-group", serviceGroup,
            "--drill", drill,
            "--drill-run", drillRun,
            "--stage", Stage);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("single non-empty path segment", response.Message);
    }

    [Theory]
    [InlineData(HttpStatusCode.Conflict, "current state")]
    [InlineData(HttpStatusCode.Forbidden, "Authorization failed")]
    [InlineData(HttpStatusCode.NotFound, "not found")]
    [InlineData(HttpStatusCode.BadRequest, "mark complete request failed")]
    public async Task ExecuteAsync_SanitizesRequestFailedException(HttpStatusCode status, string expectedMessage)
    {
        const string providerDetails = "Sensitive provider details: request-id=123; endpoint=https://example.invalid";
        Service.MarkDrillRunCompleteAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException((int)status, providerDetails));

        var response = await ExecuteCommandAsync(
            "--service-group", ServiceGroup,
            "--drill", Drill,
            "--drill-run", DrillRun,
            "--stage", Stage);

        Assert.Equal(status, response.Status);
        Assert.Contains(expectedMessage, response.Message, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(providerDetails, response.Message);
    }
}
