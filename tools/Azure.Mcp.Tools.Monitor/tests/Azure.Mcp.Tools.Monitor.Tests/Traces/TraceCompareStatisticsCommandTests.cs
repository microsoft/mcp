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

public sealed class TraceCompareStatisticsCommandTests()
    : SubscriptionCommandUnitTestsBase<TraceCompareStatisticsCommand, IMonitorTracesService>
{
    private const string Target = "--subscription sub --resource-group rg --workspace amw --start-time 2026-01-02T00:00:00Z --end-time 2026-01-02T01:00:00Z";

    [Theory]
    [InlineData("--query Traces --compare-query Traces", true)]
    [InlineData("--query Traces --compare-start-time 2026-01-01T00:00:00Z --compare-end-time 2026-01-01T01:00:00Z", true)]
    [InlineData("--compare-query Traces --compare-start-time 2026-01-01T00:00:00Z --compare-end-time 2026-01-01T01:00:00Z", false)]
    [InlineData("--query Traces", false)]
    [InlineData("--compare-query Traces", false)]
    [InlineData("--query Traces --compare-start-time 2026-01-01T00:00:00Z", false)]
    [InlineData("--query Traces --compare-query Traces --include-samples false", false)]
    [InlineData("--query Traces --compare-query Traces --compare-start-time 2025-12-30T00:00:00Z --compare-end-time 2026-01-01T01:00:00Z", false)]
    public async Task ValidatesIndependentPopulationsBeforeService(string args, bool valid)
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
            var result = ValidateAndDeserializeResponse(response, MonitorJsonContext.Default.TraceQueryCommandResult);
            Assert.Equal("Succeeded", result.Result.Status);
            await Service.Received(1).ComparePopulationsAsync("sub", "rg", "amw", "statistics",
                Arg.Is<TraceComparisonRequest>(request => request.Current.Options!.MaxResults == 10000),
                null, Arg.Any<string?>(), Arg.Any<CancellationToken>());
        }
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(10001, 10000)]
    public async Task ClampsMaxResults(int requested, int expected)
    {
        Service.ComparePopulationsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<TraceComparisonRequest>(), Arg.Any<string?>(), Arg.Any<string?>(),
            Arg.Any<CancellationToken>()).Returns(new TraceQueryResult { Status = "Succeeded", OperationId = "operation" });

        var response = await ExecuteCommandAsync($"{Target} --query Traces --compare-query Traces --max-results {requested}");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).ComparePopulationsAsync("sub", "rg", "amw", "statistics",
            Arg.Is<TraceComparisonRequest>(request => request.Current.Options!.MaxResults == expected),
            null, Arg.Any<string?>(), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest)]
    [InlineData(HttpStatusCode.NotFound)]
    [InlineData(HttpStatusCode.NotImplemented)]
    public async Task SurfacesBackendRejectionWithoutFallback(HttpStatusCode status)
    {
        Service.ComparePopulationsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<TraceComparisonRequest>(), Arg.Any<string?>(), Arg.Any<string?>(),
            Arg.Any<CancellationToken>()).ThrowsAsync(new HttpRequestException("Comparison feature disabled or invalid shape.", null, status));
        var response = await ExecuteCommandAsync($"{Target} --query Traces --compare-query Traces");
        Assert.Equal(status, response.Status);
    }

    [Fact]
    public async Task PreservesFractionalBoundsForBothPopulations()
    {
        Service.ComparePopulationsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<TraceComparisonRequest>(), Arg.Any<string?>(), Arg.Any<string?>(),
            Arg.Any<CancellationToken>()).Returns(new TraceQueryResult { Status = "Succeeded", OperationId = "operation" });
        var response = await ExecuteCommandAsync("--subscription sub --resource-group rg --workspace amw --query Traces "
            + "--start-time 2026-01-02T00:00:00.100Z --end-time 2026-01-02T00:00:00.900Z "
            + "--compare-start-time 2026-01-01T00:00:00.1234567Z --compare-end-time 2026-01-01T00:00:00.9876543Z");
        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).ComparePopulationsAsync("sub", "rg", "amw", "statistics",
            Arg.Is<TraceComparisonRequest>(request =>
                request.Current.TimeRange!.StartTime == "2026-01-02T00:00:00.1Z"
                && request.Current.TimeRange.EndTime == "2026-01-02T00:00:00.9Z"
                && request.CompareTo.TimeRange!.StartTime == "2026-01-01T00:00:00.1234567Z"
                && request.CompareTo.TimeRange.EndTime == "2026-01-01T00:00:00.9876543Z"),
            null, Arg.Any<string?>(), Arg.Any<CancellationToken>());
    }
}
