// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Azure.Mcp.Tools.ResilienceManagement.Commands;
using Azure.Mcp.Tools.ResilienceManagement.Commands.Drills.Runs;
using Azure.Mcp.Tools.ResilienceManagement.Models;
using Azure.Mcp.Tools.ResilienceManagement.Services;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.ResilienceManagement.Tests.Drills.Runs;

public sealed class DrillRunGenerateReportCommandTests
    : CommandUnitTestsBase<DrillRunGenerateReportCommand, IResilienceManagementService>
{
    private const string ServiceGroup = "sg1";
    private const string Drill = "drill1";
    private const string DrillRun = "run1";

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = Command.GetCommand();

        Assert.Equal("generate-report", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Theory]
    [InlineData("--drill drill1 --drill-run run1", false)]
    [InlineData("--service-group sg1 --drill-run run1", false)]
    [InlineData("--service-group sg1 --drill drill1", false)]
    public async Task ExecuteAsync_ValidatesRequiredInput(string args, bool shouldSucceed)
    {
        var response = await ExecuteCommandAsync(args);

        Assert.Equal(shouldSucceed ? HttpStatusCode.OK : HttpStatusCode.BadRequest, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsOperationWhenGenerationIsStillRunning()
    {
        Service.GenerateDrillRunReportAsync(ServiceGroup, Drill, DrillRun, "tenant1", Arg.Any<CancellationToken>())
            .Returns(new DrillRunGenerateReportResult("operation1", false, null));

        var response = await ExecuteCommandAsync(
            "--service-group", ServiceGroup,
            "--drill", Drill,
            "--drill-run", DrillRun,
            "--tenant", "tenant1");

        var result = ValidateAndDeserializeResponse(
            response,
            ResilienceManagementJsonContext.Default.DrillRunGenerateReportCommandResult);
        Assert.Equal("operation1", result.Result.OperationId);
        Assert.False(result.Result.HasCompleted);
        Assert.Null(result.Result.Report);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsReportSummaryWhenGenerationCompleted()
    {
        var report = new DrillRunReportInfo(
            "Succeeded",
            "NotFinalized",
            [new DrillRunReportStageInfo("FaultInjection", "Succeeded", DateTimeOffset.Parse("2026-10-22T01:00:00Z"), null)],
            ["Html"],
            DateTimeOffset.Parse("2026-10-22T01:00:00Z"),
            "1.0",
            null);
        Service.GenerateDrillRunReportAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(new DrillRunGenerateReportResult("operation1", true, report));

        var response = await ExecuteCommandAsync(
            "--service-group", ServiceGroup,
            "--drill", Drill,
            "--drill-run", DrillRun);

        var result = ValidateAndDeserializeResponse(
            response,
            ResilienceManagementJsonContext.Default.DrillRunGenerateReportCommandResult);
        Assert.True(result.Result.HasCompleted);
        Assert.NotNull(result.Result.Report);
        Assert.Equal("Succeeded", result.Result.Report.GenerationStatus);
        Assert.Equal("FaultInjection", Assert.Single(result.Result.Report.StageStatuses).DrillRunStage);
        Assert.Equal("Html", Assert.Single(result.Result.Report.AvailableFormats));
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
            "--drill-run", drillRun);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("single non-empty path segment", response.Message);
        await Service.DidNotReceive().GenerateDrillRunReportAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(HttpStatusCode.Forbidden, "Authorization failed")]
    [InlineData(HttpStatusCode.NotFound, "not found")]
    [InlineData(HttpStatusCode.NotImplemented, "report storage is not configured")]
    [InlineData(HttpStatusCode.ServiceUnavailable, "dependency did not respond")]
    [InlineData(HttpStatusCode.BadRequest, "report generation request failed")]
    public async Task ExecuteAsync_SanitizesRequestFailedException(HttpStatusCode status, string expectedMessage)
    {
        const string providerDetails = "Sensitive provider details: request-id=123; endpoint=https://example.invalid";
        Service.GenerateDrillRunReportAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException((int)status, providerDetails));

        var response = await ExecuteCommandAsync(
            "--service-group", ServiceGroup,
            "--drill", Drill,
            "--drill-run", DrillRun);

        Assert.Equal(status, response.Status);
        Assert.Contains(expectedMessage, response.Message, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(providerDetails, response.Message);
        Assert.Null(response.Results);
        Assert.DoesNotContain("Sensitive provider details", JsonSerializer.Serialize(response));
    }
}
