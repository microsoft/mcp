// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tests.Commands;
using Azure.Mcp.Tools.Monitor.Commands.Traces;
using Azure.Mcp.Tools.Monitor.Models.Traces;
using Azure.Mcp.Tools.Monitor.Options.Traces;
using Azure.Mcp.Tools.Monitor.Services.Traces;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Mcp.Core.Commands;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.Monitor.Tests.Traces;

public sealed class TraceCompareCriticalPathCommandTests()
    : SubscriptionCommandUnitTestsBase<TraceCompareCriticalPathCommand, IMonitorTracesService>
{
    private const string Id = "0123456789abcdef0123456789abcdef";
    private const string Target = "--subscription sub --resource-group rg --workspace amw --start-time 2026-01-02T00:00:00Z --end-time 2026-01-02T01:00:00Z";

    [Fact]
    public void ValidateOptions_CollectsErrorsForBothPopulationQueries()
    {
        var command = new TraceCompareCriticalPathCommand(
            NullLogger<TraceCompareCriticalPathCommand>.Instance, Service, SubscriptionResolver);
        var validation = new ValidationResult();
        command.ValidateOptions(new TraceCompareCriticalPathOptions
        {
            Subscription = "sub",
            Workspace = "amw",
            ResourceGroup = "rg",
            StartTime = "2026-01-02T00:00:00Z",
            EndTime = "2026-01-02T01:00:00Z",
            Query = "Traces // comment",
            CompareQuery = "Traces | where root_span_name == 'unterminated",
        }, validation);

        Assert.Equal(2, validation.Errors.Count);
        Assert.Contains(validation.Errors, error => error.Contains("current", StringComparison.Ordinal)
            && error.Contains("comments", StringComparison.Ordinal));
        Assert.Contains(validation.Errors, error => error.Contains("comparison", StringComparison.Ordinal)
            && error.Contains("unterminated", StringComparison.Ordinal));
        Assert.Empty(Service.ReceivedCalls());
    }

    [Theory]
    [InlineData($"--trace-ids {Id} --compare-trace-ids {Id}", true)]
    [InlineData("--trace-ids 01234567-89ab-cdef-0123-456789abcdef --compare-trace-ids 01234567-89ab-cdef-0123-456789abcdef", true)]
    [InlineData("--query Traces --compare-query Traces", true)]
    [InlineData("--query \"Traces | where root_span_name == 'GET|POST'\" --compare-query \"Traces | where root_span_name == '$$'\"", true)]
    [InlineData("--query \"Traces | where root_span_name == 'it''s|POST$$'\" --compare-query \"Traces | where root_span_name == @'it''s|POST$$'\"", true)]
    [InlineData("--query \"Traces | where duration_nano > 1000000 $$ <<Traces | where ['service.name'] == 'gateway' $$ Traces | where ['service.name'] == 'backend'>>\" --compare-query Traces", true)]
    [InlineData("--query \"T | where duration_nano > 1000000 $$ << T | where ['service.name'] == 'gateway' $$ T | where ['service.name'] == 'backend' >>\" --compare-query T", true)]
    [InlineData($"--trace-ids {Id} --compare-query Traces", true)]
    [InlineData($"--trace-ids {Id} --query Traces --compare-trace-ids {Id}", false)]
    [InlineData($"--trace-ids invalid --compare-trace-ids {Id}", false)]
    [InlineData($"--trace-ids {Id}", false)]
    [InlineData("--query \"Traces | project trace_id\" --compare-query Traces", false)]
    [InlineData("--query \"Traces | summarize count=count() by trace_id\" --compare-query Traces", false)]
    [InlineData("--query \"Traces $$ <<Traces>> $$ <<Traces>>\" --compare-query Traces", false)]
    [InlineData("--query \"Traces $$ Traces\" --compare-query Traces", false)]
    [InlineData("--query Traces --compare-query Traces --aggregate-by evil/field", false)]
    [InlineData("--query Traces --compare-query Traces --aggregate-by \"span.name'] | project trace_id\"", false)]
    [InlineData("--query Traces --compare-query Traces --aggregate-by \"span.attributes.key'] | project trace_id\"", false)]
    [InlineData("--query Traces --compare-query Traces --aggregate-by span.attributes.key\\", false)]
    [InlineData("--query Traces --compare-query Traces --percentiles 100", false)]
    [InlineData("--query Traces --compare-query Traces --percentiles 0", false)]
    public async Task ValidatesIdsFiltersAndGrouping(string args, bool valid)
    {
        Service.ComparePopulationsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<TraceComparisonRequest>(), Arg.Any<string?>(), Arg.Any<string?>(),
            Arg.Any<CancellationToken>()).Returns(new TraceQueryResult { Status = "Succeeded", OperationId = "operation" });
        var response = await ExecuteCommandAsync($"{Target} {args}");
        Assert.Equal(valid ? HttpStatusCode.OK : HttpStatusCode.BadRequest, response.Status);
        if (!valid)
        {
            Assert.Empty(Service.ReceivedCalls());
        }
        else
        {
            await Service.Received(1).ComparePopulationsAsync("sub", "rg", "amw", "critical-path",
                Arg.Is<TraceComparisonRequest>(r => r.Current.Options!.IncludeSamples == true
                    && r.Current.Options.ResolveTraceLinks == false && r.Current.Options.MaxResults == 10000),
                "Traces | summarize p50=percentile(duration_nano, 50), p95=percentile(duration_nano, 95), p99=percentile(duration_nano, 99) by ['span.name']",
                Arg.Any<string?>(), Arg.Any<CancellationToken>());
        }
    }

    [Fact]
    public async Task ExecuteAsync_NormalizesGuidFormatsContainingCommas()
    {
        Service.ComparePopulationsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<TraceComparisonRequest>(), Arg.Any<string?>(), Arg.Any<string?>(),
            Arg.Any<CancellationToken>()).Returns(new TraceQueryResult { Status = "Succeeded", OperationId = "operation" });
        string id = Guid.Parse(Id).ToString("X");

        var response = await ExecuteCommandAsync($"{Target} --trace-ids \"{id}\" --compare-trace-ids {Id}");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).ComparePopulationsAsync("sub", "rg", "amw", "critical-path",
            Arg.Is<TraceComparisonRequest>(request => request.Current.TraceId == Id && request.CompareTo.TraceId == Id),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("--query Traces --compare-query Traces --exclude-subtree-prefixes \" \"", "Exclude subtree prefixes must not be empty or whitespace.")]
    [InlineData("--query Traces --compare-query Traces --deprioritize-subtree-prefixes \" \"", "Deprioritize subtree prefixes must not be empty or whitespace.")]
    [InlineData("--query Traces --compare-query Traces --exclude-subtree-prefixes sql. --deprioritize-subtree-prefixes sql.", "The exact same subtree prefix cannot appear in both prefix lists.")]
    public async Task RejectsInvalidSubtreePrefixCombinations(string args, string error)
    {
        var response = await ExecuteCommandAsync($"{Target} {args}");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains(error, response.Message, StringComparison.Ordinal);
        Assert.Empty(Service.ReceivedCalls());
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(10001, 10000)]
    [InlineData(100000, 10000)]
    public async Task ClampsMaxResultsToComparisonLimit(int requested, int expected)
    {
        Service.ComparePopulationsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<TraceComparisonRequest>(), Arg.Any<string?>(), Arg.Any<string?>(),
            Arg.Any<CancellationToken>()).Returns(new TraceQueryResult { Status = "Succeeded", OperationId = "operation" });

        var response = await ExecuteCommandAsync($"{Target} --query Traces --compare-query Traces --max-results {requested}");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).ComparePopulationsAsync("sub", "rg", "amw", "critical-path",
            Arg.Is<TraceComparisonRequest>(request => request.Current.Options!.MaxResults == expected),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AcceptsMoreThanFiveValidPercentiles()
    {
        Service.ComparePopulationsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<TraceComparisonRequest>(), Arg.Any<string?>(), Arg.Any<string?>(),
            Arg.Any<CancellationToken>()).Returns(new TraceQueryResult { Status = "Succeeded", OperationId = "operation" });

        var response = await ExecuteCommandAsync(
            $"{Target} --query Traces --compare-query Traces --percentiles 1 2 3 4 5 6");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).ComparePopulationsAsync("sub", "rg", "amw", "critical-path",
            Arg.Any<TraceComparisonRequest>(),
            "Traces | summarize p1=percentile(duration_nano, 1), p2=percentile(duration_nano, 2), p3=percentile(duration_nano, 3), p4=percentile(duration_nano, 4), p5=percentile(duration_nano, 5), p6=percentile(duration_nano, 6) by ['span.name']",
            Arg.Any<string?>(), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("['span.name'] span.service.name", "['span.name'], ['span.service.name']")]
    [InlineData("span.attributes.http.request.header.user-agent", "['span.attributes.http.request.header.user-agent']")]
    [InlineData("['span.attributes.http.request.header.user-agent']", "['span.attributes.http.request.header.user-agent']")]
    [InlineData("span.attributes.guid:x-request-id", "['span.attributes.guid:x-request-id']")]
    [InlineData("span.attributes.k8s.pod.label.app.kubernetes.io/name", "['span.attributes.k8s.pod.label.app.kubernetes.io/name']")]
    [InlineData("['span.attributes.k8s.pod.label.app.kubernetes.io/name']", "['span.attributes.k8s.pod.label.app.kubernetes.io/name']")]
    public async Task ExecuteAsync_NormalizesGroupingBeforeBuildingCustomPercentiles(string fields, string expected)
    {
        Service.ComparePopulationsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<TraceComparisonRequest>(), Arg.Any<string?>(), Arg.Any<string?>(),
            Arg.Any<CancellationToken>()).Returns(new TraceQueryResult { Status = "Succeeded", OperationId = "operation" });

        var response = await ExecuteCommandAsync(
            $"{Target} --query Traces --compare-query Traces --aggregate-by {fields} --percentiles 25 90");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).ComparePopulationsAsync("sub", "rg", "amw", "critical-path",
            Arg.Any<TraceComparisonRequest>(),
            $"Traces | summarize p25=percentile(duration_nano, 25), p90=percentile(duration_nano, 90) by {expected}",
            Arg.Any<string?>(), Arg.Any<CancellationToken>());
    }
}
