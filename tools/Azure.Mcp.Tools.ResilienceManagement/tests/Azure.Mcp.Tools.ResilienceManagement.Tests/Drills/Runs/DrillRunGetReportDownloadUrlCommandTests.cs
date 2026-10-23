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

public sealed class DrillRunGetReportDownloadUrlCommandTests
    : CommandUnitTestsBase<DrillRunGetReportDownloadUrlCommand, IResilienceManagementService>
{
    private const string ServiceGroup = "sg1";
    private const string Drill = "drill1";
    private const string DrillRun = "run1";

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = Command.GetCommand();

        Assert.Equal("get-report-download-url", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Fact]
    public void Metadata_MarksCommandReadOnlyAndSecret()
    {
        var metadata = Command.Metadata;

        Assert.True(metadata.ReadOnly);
        Assert.True(metadata.Secret);
        Assert.False(metadata.Destructive);
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
    public async Task ExecuteAsync_ReturnsDownloadUrlWithDefaultFormat()
    {
        var expires = DateTimeOffset.Parse("2026-10-22T02:00:00Z");
        Service.GetDrillRunReportDownloadUrlAsync(ServiceGroup, Drill, DrillRun, null, "tenant1", Arg.Any<CancellationToken>())
            .Returns(new DrillRunReportDownloadInfo("Html", "https://example.invalid/report.html?sig=abc", expires));

        var response = await ExecuteCommandAsync(
            "--service-group", ServiceGroup,
            "--drill", Drill,
            "--drill-run", DrillRun,
            "--tenant", "tenant1");

        var result = ValidateAndDeserializeResponse(
            response,
            ResilienceManagementJsonContext.Default.DrillRunGetReportDownloadUrlCommandResult);
        Assert.Equal("Html", result.Result.Format);
        Assert.Equal("https://example.invalid/report.html?sig=abc", result.Result.DownloadUri);
        Assert.Equal(expires, result.Result.ExpiresOn);
    }

    [Fact]
    public async Task ExecuteAsync_NormalizesFormatCasing()
    {
        var response = await ExecuteCommandAsync(
            "--service-group", ServiceGroup,
            "--drill", Drill,
            "--drill-run", DrillRun,
            "--format", "html");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).GetDrillRunReportDownloadUrlAsync(
            ServiceGroup, Drill, DrillRun, "Html", Arg.Any<string?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_RejectsUnknownFormat()
    {
        var response = await ExecuteCommandAsync(
            "--service-group", ServiceGroup,
            "--drill", Drill,
            "--drill-run", DrillRun,
            "--format", "Pdf");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("--format must be one of", response.Message);
        await Service.DidNotReceive().GetDrillRunReportDownloadUrlAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>());
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
    }

    [Theory]
    [InlineData(HttpStatusCode.Forbidden, "Authorization failed")]
    [InlineData(HttpStatusCode.NotFound, "No report is available")]
    [InlineData(HttpStatusCode.NotImplemented, "report storage is not configured")]
    [InlineData(HttpStatusCode.ServiceUnavailable, "dependency did not respond")]
    [InlineData(HttpStatusCode.BadRequest, "download URL request failed")]
    public async Task ExecuteAsync_SanitizesRequestFailedException(HttpStatusCode status, string expectedMessage)
    {
        const string providerDetails = "Sensitive provider details: request-id=123; endpoint=https://example.invalid";
        Service.GetDrillRunReportDownloadUrlAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
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

    [Theory]
    [InlineData("ReportFailed", "(ReportFailed)")]
    [InlineData(null, "could not create the report download URL.")]
    public async Task ExecuteAsync_ReportsFailedOperationWithSuccessStatusAsBadGateway(string? errorCode, string expectedMessage)
    {
        const string providerDetails = "Sensitive provider details: request-id=123";
        Service.GetDrillRunReportDownloadUrlAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException(200, providerDetails, errorCode, null));

        var response = await ExecuteCommandAsync(
            "--service-group", ServiceGroup,
            "--drill", Drill,
            "--drill-run", DrillRun);

        Assert.Equal(HttpStatusCode.BadGateway, response.Status);
        Assert.Contains(expectedMessage, response.Message);
        Assert.DoesNotContain(providerDetails, response.Message);
        Assert.Null(response.Results);
        Assert.DoesNotContain(providerDetails, JsonSerializer.Serialize(response));
    }

    [Fact]
    public async Task ExecuteAsync_ReportsTimeoutAsGatewayTimeout()
    {
        Service.GetDrillRunReportDownloadUrlAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new TimeoutException("internal timing details"));

        var response = await ExecuteCommandAsync(
            "--service-group", ServiceGroup,
            "--drill", Drill,
            "--drill-run", DrillRun);

        Assert.Equal(HttpStatusCode.GatewayTimeout, response.Status);
        Assert.Contains("timed out", response.Message, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("internal timing details", response.Message);
        Assert.Null(response.Results);
        Assert.DoesNotContain("internal timing details", JsonSerializer.Serialize(response));
    }
}
