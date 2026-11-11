// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tests.Commands;
using Azure.Mcp.Tools.Monitor.Commands;
using Azure.Mcp.Tools.Monitor.Commands.Traces;
using Azure.Mcp.Tools.Monitor.Models.Traces;
using Azure.Mcp.Tools.Monitor.Services.Traces;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.Monitor.Tests.Traces;

public sealed class TraceAnalyzeCriticalPathCommandTests : SubscriptionCommandUnitTestsBase<TraceAnalyzeCriticalPathCommand, IMonitorTracesService>
{
    private const string Id = "abcdef1234567890abcdef1234567890";
    private const string Target = "--subscription sub --resource-group rg --workspace amw --tenant tenant";
    private const string Window = "--start-time 2026-10-12T00:00:00Z --end-time 2026-10-12T01:00:00Z";

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ExecuteAsync_BuildsAggregationAndForwardsSettings(bool custom)
    {
        SubscriptionResolver.ResolveSubscription(null).Returns("sub");
        Service.CriticalPathAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>(), Arg.Any<string[]>(), Arg.Any<TraceQueryOptionsBody>(),
            Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(new TraceQueryResult { Status = "Running", OperationId = "op-critical" });

        var args = custom
            ? "--aggregate-by span.service.name span.name span.attributes.guid_x_request_id --percentiles 90 99 --exclude-subtree-prefixes sql. redis. --deprioritize-subtree-prefixes background. --resolve-trace-links true --include-samples false --include-rollups true --max-results 1000"
            : "";
        var target = custom ? Target : "--resource-group rg --workspace amw";
        var response = await ExecuteCommandAsync($"{target} {Window} --trace-ids {Id} {args}");
        var result = ValidateAndDeserializeResponse(response, MonitorJsonContext.Default.TraceQueryCommandResult);

        Assert.Equal("Running", result.Result.Status);
        Assert.Equal("op-critical", result.Result.OperationId);
        var expectedQuery = custom
            ? "Traces | summarize p90=percentile(duration_nano, 90), p99=percentile(duration_nano, 99) by ['span.service.name'], ['span.name'], ['span.attributes.guid_x_request_id']"
            : "Traces | summarize p50=percentile(duration_nano, 50), p95=percentile(duration_nano, 95), p99=percentile(duration_nano, 99) by ['span.name']";
        await Service.Received(1).CriticalPathAsync(
            "sub", "rg", "amw", expectedQuery, Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>(),
            Arg.Is<string[]>(ids => ids.SequenceEqual(new[] { Id })),
            Arg.Is<TraceQueryOptionsBody>(settings => settings.MaxResults == (custom ? 1000 : 10)
                && settings.IncludeSamples == !custom && settings.IncludeRollups == custom
                && settings.ResolveTraceLinks == custom
                && (!custom || (settings.ExcludeSubtreePrefixes!.SequenceEqual(new[] { "sql.", "redis." })
                    && settings.DeprioritizeSubtreePrefixes!.SequenceEqual(new[] { "background." })))),
            custom ? "tenant" : null, Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("--trace-ids abcdef1234567890abcdef1234567890 --aggregate-by \"span.name'] | project trace_id\"")]
    [InlineData("--trace-ids abcdef1234567890abcdef1234567890 --aggregate-by \"span.attributes.key'\"")]
    [InlineData("--trace-ids abcdef1234567890abcdef1234567890 --aggregate-by \"span.attributes.key\\\"")]
    [InlineData("--trace-ids abcdef1234567890abcdef1234567890 --percentiles 0")]
    [InlineData("--trace-ids abcdef1234567890abcdef1234567890 --percentiles 100")]
    [InlineData("--trace-ids abcdef1234567890abcdef1234567890 --percentiles 101")]
    [InlineData("--trace-ids abcdef1234567890abcdef1234567890 --aggregate-by span.attributes.guid:x-request-id")]
    [InlineData("")]
    public async Task ExecuteAsync_RejectsInvalidArguments(string args)
    {
        var response = await ExecuteCommandAsync($"{Target} {Window} {args}");
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Empty(Service.ReceivedCalls());
    }

    [Theory]
    [InlineData("--exclude-subtree-prefixes \" \"", "Exclude subtree prefixes must not be empty or whitespace.")]
    [InlineData("--deprioritize-subtree-prefixes \" \"", "Deprioritize subtree prefixes must not be empty or whitespace.")]
    [InlineData("--exclude-subtree-prefixes sql. --deprioritize-subtree-prefixes sql.", "The exact same subtree prefix cannot appear in both prefix lists.")]
    public async Task ExecuteAsync_RejectsInvalidSubtreePrefixes(string args, string error)
    {
        var response = await ExecuteCommandAsync($"{Target} {Window} --trace-ids {Id} {args}");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains(error, response.Message, StringComparison.Ordinal);
        Assert.Empty(Service.ReceivedCalls());
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(1, 1)]
    [InlineData(1000, 1000)]
    [InlineData(1001, 1000)]
    public async Task ExecuteAsync_ClampsMaxResults(int requested, int expected)
    {
        Service.CriticalPathAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>(), Arg.Any<string[]>(), Arg.Any<TraceQueryOptionsBody>(),
            Arg.Any<string?>(), Arg.Any<CancellationToken>()).Returns(new TraceQueryResult { Status = "Succeeded", OperationId = "op-critical" });

        var response = await ExecuteCommandAsync($"{Target} {Window} --trace-ids {Id} --max-results {requested}");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).CriticalPathAsync(
            "sub", "rg", "amw", Arg.Any<string>(), Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>(),
            Arg.Any<string[]>(), Arg.Is<TraceQueryOptionsBody>(body => body.MaxResults == expected),
            "tenant", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_ForwardsBackendValidatedInputs()
    {
        Service.CriticalPathAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>(), Arg.Any<string[]>(), Arg.Any<TraceQueryOptionsBody>(),
            Arg.Any<string?>(), Arg.Any<CancellationToken>()).Returns(new TraceQueryResult { Status = "Succeeded", OperationId = "op-critical" });

        var response = await ExecuteCommandAsync($"{Target} {Window} --trace-ids invalid --aggregate-by a b c d --percentiles 1 2 3 4 5 6");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).CriticalPathAsync(
            "sub", "rg", "amw",
            Arg.Is<string>(query => query.Contains("p6=percentile(duration_nano, 6)", StringComparison.Ordinal)
                && query.EndsWith("['a'], ['b'], ['c'], ['d']", StringComparison.Ordinal)),
            Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>(),
            Arg.Is<string[]>(ids => ids.SequenceEqual(new[] { "invalid" })), Arg.Any<TraceQueryOptionsBody>(),
            "tenant", Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("--resource-group rg --workspace amw")]
    [InlineData("--subscription sub --workspace amw")]
    [InlineData("--subscription sub --resource-group rg")]
    public async Task ExecuteAsync_RejectsMissingResourceContext(string target)
    {
        var response = await ExecuteCommandAsync($"{target} {Window} --trace-ids {Id}");
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Empty(Service.ReceivedCalls());
    }

    [Theory]
    [InlineData("2026-10-12T00:00:00Z", "2026-10-12T00:00:00Z")]
    [InlineData("2026-10-10T00:00:00Z", "2026-10-12T00:00:00Z")]
    [InlineData("invalid", "2026-10-12T00:00:00Z")]
    public async Task ExecuteAsync_RejectsInvalidWindow(string start, string end)
    {
        var response = await ExecuteCommandAsync($"{Target} --start-time {start} --end-time {end} --trace-ids {Id}");
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Empty(Service.ReceivedCalls());
    }
}
