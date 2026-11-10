// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tests.Commands;
using Azure.Mcp.Tools.Monitor.Commands;
using Azure.Mcp.Tools.Monitor.Commands.Traces;
using Azure.Mcp.Tools.Monitor.Models.Traces;
using Azure.Mcp.Tools.Monitor.Services.Traces;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Monitor.Tests.Traces;

public sealed class TraceAnalyzeTopAttributesCommandTests : SubscriptionCommandUnitTestsBase<TraceAnalyzeTopAttributesCommand, IMonitorTracesService>
{
    private const string Target = "--subscription sub --resource-group rg --workspace amw --tenant tenant";
    private const string Window = "--overall-start-time 2026-10-12T00:00:00Z --overall-end-time 2026-10-12T01:00:00Z --selection-start-time 2026-10-12T00:30:00Z --selection-end-time 2026-10-12T01:00:00Z";
    private const string Selection = "--selection-field span_count --selection-op > --selection-value 10";

    [Fact]
    public void Constructor_UsesTopAttributesName()
    {
        Assert.Equal("analyze-top-attributes", Command.GetCommand().Name);
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(1, 1)]
    [InlineData(1000, 1000)]
    [InlineData(10000, 10000)]
    [InlineData(10001, 10000)]
    public async Task ExecuteAsync_ClampsAndForwardsTraceSample(int requested, int expected)
    {
        Service.CompareAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>(), Arg.Any<int>(), Arg.Any<string?>(),
            Arg.Any<CancellationToken>()).Returns(new TraceQueryResult { Status = "Running", OperationId = "op-compare" });

        var response = await ExecuteCommandAsync($"{Target} {Window} {Selection} --max-results {requested}");
        var result = ValidateAndDeserializeResponse(response, MonitorJsonContext.Default.TraceQueryCommandResult);

        Assert.Equal("Running", result.Result.Status);
        Assert.Equal("op-compare", result.Result.OperationId);
        await Service.Received(1).CompareAsync("sub", "rg", "amw",
            "Traces | compare(span_count > 10, 8, 2026-10-12T00:30:00.0000000Z, 2026-10-12T01:00:00.0000000Z)",
            Arg.Is<DateTimeOffset>(time => time.Hour == 0),
            Arg.Is<DateTimeOffset>(time => time.Hour == 1), expected, "tenant", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_NormalizesSpanFilterAndPreservesQuotedSelection()
    {
        Service.CompareAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>(), Arg.Any<int>(), Arg.Any<string?>(),
            Arg.Any<CancellationToken>()).Returns(new TraceQueryResult { Status = "Succeeded", OperationId = "op-compare" });

        var response = await ExecuteCommandAsync($"{Target} {Window} --overall-query \"<<Traces | where ['span.name'] == 'GET'>>\" --selection-field span.eval.result --selection-op == --selection-value \"'fail'\" --top-n 100");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).CompareAsync("sub", "rg", "amw",
            Arg.Is<string>(query => query.StartsWith("Traces $$ <<Traces", StringComparison.Ordinal)
                && query.Contains("| compare(span.eval.result == 'fail', 100,", StringComparison.Ordinal)),
            Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>(), 10000, "tenant", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_ForwardsBackendValidatedTopNAndQuery()
    {
        Service.CompareAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>(), Arg.Any<int>(), Arg.Any<string?>(),
            Arg.Any<CancellationToken>()).Returns(new TraceQueryResult { Status = "Succeeded", OperationId = "op-compare" });
        var query = "Traces | where span_count > 0" + new string(' ', 65536) + "| project trace_id";

        var response = await ExecuteCommandAsync($"{Target} {Window} {Selection} --overall-query \"{query}\" --top-n 101");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).CompareAsync("sub", "rg", "amw",
            Arg.Is<string>(value => value.StartsWith(query, StringComparison.Ordinal)
                && value.Contains("compare(span_count > 10, 101,", StringComparison.Ordinal)),
            Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>(), 10000, "tenant", Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("span_count", "in", "10")]
    [InlineData("['span.name']", "==", "'fail'")]
    [InlineData("span.attributes.guid:x-request-id", "==", "'fail'")]
    [InlineData("span_count", ">", "NaN")]
    [InlineData("span_count", ">", "Infinity")]
    [InlineData("span_count", ">", "10) | project trace_id")]
    [InlineData("span.name", "==", "'fail') | project trace_id")]
    public async Task ExecuteAsync_RejectsUnsafeSelection(string field, string op, string value)
    {
        var response = await ExecuteCommandAsync($"{Target} {Window} --selection-field \"{field}\" --selection-op {op} --selection-value \"{value}\"");
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Empty(Service.ReceivedCalls());
    }

    [Theory]
    [InlineData("--overall-start-time 2026-10-10T00:00:00Z --overall-end-time 2026-10-12T01:00:00Z --selection-start-time 2026-10-12T00:00:00Z --selection-end-time 2026-10-12T01:00:00Z")]
    [InlineData("--overall-start-time 2026-10-12T00:00:00Z --overall-end-time 2026-10-12T01:00:00Z --selection-start-time 2026-10-11T23:00:00Z --selection-end-time 2026-10-12T01:00:00Z")]
    [InlineData("--overall-start-time invalid --overall-end-time 2026-10-12T01:00:00Z --selection-start-time 2026-10-12T00:00:00Z --selection-end-time 2026-10-12T01:00:00Z")]
    [InlineData("")]
    public async Task ExecuteAsync_RejectsInvalidOrMissingWindows(string window)
    {
        var response = await ExecuteCommandAsync($"{Target} {window} {Selection}");
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Empty(Service.ReceivedCalls());
    }

    [Theory]
    [InlineData("--overall-start-time 2026-10-12T00:00:00Z")]
    [InlineData("--overall-end-time 2026-10-12T01:00:00Z")]
    [InlineData("--selection-start-time 2026-10-12T00:30:00Z")]
    [InlineData("--selection-end-time 2026-10-12T01:00:00Z")]
    [InlineData("--selection-field span_count")]
    [InlineData("--selection-op >")]
    [InlineData("--selection-value 10")]
    public async Task ExecuteAsync_RejectsMissingRequiredAnalysisInput(string omitted)
    {
        var args = $"{Target} {Window} {Selection}".Replace(omitted, "", StringComparison.Ordinal);
        var response = await ExecuteCommandAsync(args);
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Empty(Service.ReceivedCalls());
    }

    [Fact]
    public async Task ExecuteAsync_MapsServiceFailure()
    {
        Service.CompareAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>(), Arg.Any<int>(), Arg.Any<string?>(),
            Arg.Any<CancellationToken>()).ThrowsAsync(new HttpRequestException("private backend body", null, HttpStatusCode.Forbidden));

        var response = await ExecuteCommandAsync($"{Target} {Window} {Selection}");
        Assert.Equal(HttpStatusCode.Forbidden, response.Status);
        Assert.Contains("read access", response.Message);
        Assert.DoesNotContain("private backend body", response.Message);
    }
}
