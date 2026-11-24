// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Azure.Core;
using Azure.Mcp.Tools.Monitor.Models.Traces;
using Azure.Mcp.Tools.Monitor.Services.Traces;
using Microsoft.Mcp.Core.Services.Time;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.Monitor.Tests.Traces;

public sealed class TraceComparisonServiceTests()
{
    private const string Id = "0123456789abcdef0123456789abcdef";
    private const string ComparisonId = "fedcba9876543210fedcba9876543210";

    [Fact]
    public async Task CriticalPath_RefreshesExpiredTokenAcrossDiscoveryAndComparison()
    {
        using var current = Page("""{"value":[{"trace_id":"a"}]}""");
        using var comparison = Page("""{"value":[{"trace_id":"b"}]}""");
        using var handler = new MonitorTracesTestHandler(1, false, resultPages: [current, comparison]);
        var (service, azureService) = MonitorTracesServiceTests.CreateService(handler);
        TokenCredential credential = await azureService.GetTokenCredentialAsync(null, TestContext.Current.CancellationToken);
        credential.GetTokenAsync(
            Arg.Is<TokenRequestContext>(context => context.Scopes.Contains(MonitorTracesService.TraceDataPlaneScope)),
            Arg.Any<CancellationToken>()).Returns(
            new AccessToken("expired-token", DateTimeOffset.UtcNow.AddSeconds(-1)),
            new AccessToken("fresh-token", DateTimeOffset.UtcNow.AddHours(1)));

        await service.ComparePopulationsAsync(MonitorTracesTestHandler.Subscription, "rg", "amw0",
            "critical-path", Request(false), "Traces | summarize count=count() by ['span.name']",
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal("Bearer fresh-token", handler.DataPlaneAuthorizationHeaders[^1]);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public async Task Comparison_ReportsAnInFlightPollTimeoutAsUncertain(int stalledPoll)
    {
        using var first = Page("""{"value":[{"id":"first"}],"nextLink":"more"}""");
        using var handler = new MonitorTracesTestHandler(1, false, resultPages: [first], stallPollOnCall: stalledPoll);
        IDateTimeProvider clock = Substitute.For<IDateTimeProvider>();
        DateTime now = DateTime.UtcNow;
        clock.UtcNow.Returns(now, now.AddSeconds(179.5));
        var (service, _) = MonitorTracesServiceTests.CreateService(handler, clock);

        var error = await Assert.ThrowsAsync<TraceQueryException>(() => service.ComparePopulationsAsync(
            MonitorTracesTestHandler.Subscription, "rg", "amw0", "statistics", Request(false),
            cancellationToken: TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.GatewayTimeout, error.StatusCode);
        Assert.Equal("Failed", error.PartialResult.Status);
        Assert.Contains("uncertain", error.Message, StringComparison.Ordinal);
        if (stalledPoll == 1)
        {
            Assert.Null(error.PartialResult.Payload);
        }
        else
        {
            Assert.Equal("first", error.PartialResult.Payload!.Value.GetProperty("value")[0].GetProperty("id").GetString());
        }
    }

    [Fact]
    public async Task Comparison_ReportsFirstPageHttpClientTimeoutAsUncertain()
    {
        using var handler = new MonitorTracesTestHandler(1, false, stallPollOnCall: 1);
        var (service, azureService) = MonitorTracesServiceTests.CreateService(handler);
        azureService.GetClient(Arg.Any<string?>()).Returns(_ =>
            new HttpClient(handler, disposeHandler: false) { Timeout = TimeSpan.FromMilliseconds(100) });

        var error = await Assert.ThrowsAsync<TraceQueryException>(() => service.ComparePopulationsAsync(
            MonitorTracesTestHandler.Subscription, "rg", "amw0", "statistics", Request(false),
            cancellationToken: TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.GatewayTimeout, error.StatusCode);
        Assert.Equal("op-1234", error.PartialResult.OperationId);
        Assert.Null(error.PartialResult.Payload);
        Assert.Contains("Restart", error.Message, StringComparison.Ordinal);
        Assert.Equal(1, handler.PollCalls);
    }

    [Fact]
    public async Task Comparison_RemainsResumableWhenPollDelayTimesOut()
    {
        using var pending = new HttpResponseMessage(HttpStatusCode.Accepted);
        using var handler = new MonitorTracesTestHandler(1, false, resultPages: [pending]);
        IDateTimeProvider clock = Substitute.For<IDateTimeProvider>();
        DateTime now = DateTime.UtcNow;
        clock.UtcNow.Returns(now, now.AddSeconds(179.5), now.AddSeconds(179));
        var (service, _) = MonitorTracesServiceTests.CreateService(handler, clock);

        var result = await service.ComparePopulationsAsync(
            MonitorTracesTestHandler.Subscription, "rg", "amw0", "statistics", Request(false),
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal("Running", result.Status);
        Assert.Equal("op-1234", result.OperationId);
        Assert.Equal(1, handler.PollCalls);
    }

    [Theory]
    [InlineData("", "empty")]
    [InlineData("  ", "empty")]
    [InlineData("{", "malformed JSON")]
    [InlineData("[]", "operationId")]
    [InlineData("null", "operationId")]
    [InlineData("{}", "operationId")]
    [InlineData("""{"operationId":42}""", "operationId")]
    [InlineData("""{"operationId":" "}""", "operationId")]
    public async Task Submissions_RejectInvalidResponseBeforePolling(string body, string message)
    {
        foreach (bool comparison in new[] { false, true })
        {
            using var handler = new MonitorTracesTestHandler(1, false, submitBody: body);
            var (service, _) = MonitorTracesServiceTests.CreateService(handler);
            var exception = await Assert.ThrowsAsync<HttpRequestException>(() => comparison
                ? service.ComparePopulationsAsync(
                    MonitorTracesTestHandler.Subscription, "rg", "amw0", "statistics", Request(false),
                    cancellationToken: TestContext.Current.CancellationToken)
                : service.RunQueryAsync(
                    MonitorTracesTestHandler.Subscription, "rg", "amw0", "Traces",
                    DateTimeOffset.UtcNow.AddMinutes(-1), DateTimeOffset.UtcNow, 10, false,
                    cancellationToken: TestContext.Current.CancellationToken));

            Assert.Equal(HttpStatusCode.BadGateway, exception.StatusCode);
            Assert.Contains(message, exception.Message, StringComparison.Ordinal);
            Assert.Equal(1, handler.SubmitCalls);
            Assert.Equal(0, handler.PollCalls);
        }
    }

    [Theory]
    [InlineData("statistics")]
    [InlineData("attributes")]
    [InlineData("critical-path")]
    public async Task Comparison_UsesDedicatedEnvelopeAndPreservesAllJoinedRows(string analysis)
    {
        using var first = Page("""{"value":[{"current":{"count":5},"compareTo":null,"status":"currentOnly"}],"nextLink":"ignored","isPartialResult":true,"totalTraces":452}""");
        using var second = Page("""{"value":[{"current":null,"compareTo":{"count":450},"status":"compareToOnly"}],"isPartialResult":false}""");
        using var handler = new MonitorTracesTestHandler(1, false, resultPages: [first, second]);
        var (service, _) = MonitorTracesServiceTests.CreateService(handler);
        var request = Request(analysis == "critical-path");
        request.Current.Options = new() { MaxResults = 1 };

        var result = await service.ComparePopulationsAsync(MonitorTracesTestHandler.Subscription, "rg", "amw0",
            analysis, request, "Traces | summarize p99=percentile(duration_nano, 99) by ['span.name']",
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal($"/traces:compare-{analysis}", handler.SubmitUris.Single().AbsolutePath);
        Assert.Contains("api-version=2026-01-10-preview", handler.SubmitUris[0].Query);
        using var body = JsonDocument.Parse(handler.LastSubmitBody!);
        var current = body.RootElement.GetProperty("current");
        var comparison = body.RootElement.GetProperty("compareTo");
        Assert.Equal(MonitorTracesTestHandler.TraceAccount, body.RootElement.GetProperty("traceAccountId").GetString());
        Assert.Equal("KQL", current.GetProperty("language").GetString());
        Assert.False(current.TryGetProperty("traceAccountId", out _));
        Assert.Equal(1, current.GetProperty("options").GetProperty("maxResults").GetInt32());
        Assert.Equal("2026-01-02T00:00:00Z", current.GetProperty("timeRange").GetProperty("startTime").GetString());
        Assert.Equal("2026-01-01T00:00:00Z", comparison.GetProperty("timeRange").GetProperty("startTime").GetString());
        if (analysis == "critical-path")
        {
            Assert.Equal(Id, current.GetProperty("traceId").GetString());
            Assert.Equal(ComparisonId, comparison.GetProperty("traceId").GetString());
            Assert.False(comparison.TryGetProperty("query", out _));
        }
        else
        {
            Assert.Equal("Traces | where error_rate > 0", comparison.GetProperty("query").GetString());
        }
        Assert.Equal("Succeeded", result.Status);
        Assert.Equal(2, result.Payload!.Value.GetProperty("value").GetArrayLength());
        Assert.True(result.Payload.Value.GetProperty("isPartialResult").GetBoolean());
        Assert.Equal(452, result.Payload.Value.GetProperty("totalTraces").GetInt32());
        Assert.False(result.OutputLimitReached);

        await service.ResumeQueryAsync(MonitorTracesTestHandler.Subscription, "rg", "amw0", result.OperationId!,
            cancellationToken: TestContext.Current.CancellationToken);
        Assert.Equal(1, handler.SubmitCalls);
    }

    [Theory]
    [InlineData("statistics")]
    [InlineData("attributes")]
    public async Task Comparison_OmitsInheritedQueryAndWindow(string analysis)
    {
        using var handler = new MonitorTracesTestHandler(1, false);
        var (service, _) = MonitorTracesServiceTests.CreateService(handler);
        var request = Request(false);
        request.CompareTo = new() { Query = "Traces" };
        await service.ComparePopulationsAsync(MonitorTracesTestHandler.Subscription, "rg", "amw0",
            analysis, request, cancellationToken: TestContext.Current.CancellationToken);
        using var body = JsonDocument.Parse(handler.LastSubmitBody!);
        Assert.False(body.RootElement.GetProperty("compareTo").TryGetProperty("timeRange", out _));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(101)]
    public async Task Comparison_RejectsInvalidAccountIdBeforeDataPlaneSubmit(int length)
    {
        using var handler = new MonitorTracesTestHandler(1, false,
            accountId: new string(length == 1 ? ' ' : 'a', length));
        var (service, _) = MonitorTracesServiceTests.CreateService(handler);
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.ComparePopulationsAsync(
            MonitorTracesTestHandler.Subscription, "rg", "amw0", "statistics", Request(false),
            cancellationToken: TestContext.Current.CancellationToken));
        Assert.Equal(0, handler.DataPlaneCalls);
    }

    [Theory]
    [InlineData("false")]
    public async Task CriticalPath_DiscoveryDeduplicatesAndPadsIdsWithoutReplacingAggregation(string flag)
    {
        using var current = Page($$$$"""{"value":[{"trace_id":"a"},{"trace_id":"a"}],"metadata":{"hasLimitReached":{{{{flag}}}}}}""");
        using var comparison = Page("""{"value":[{"trace_id":"b"}]}""");
        using var handler = new MonitorTracesTestHandler(1, false, resultPages: [current, comparison]);
        var (service, _) = MonitorTracesServiceTests.CreateService(handler);
        var request = Request(false);
        request.Current.Query = "Traces | where duration_nano > 10 $$ <<Traces | where ['span.name'] == 'GET'>>";
        const string aggregation = "Traces | summarize p99=percentile(duration_nano, 99) by ['span.name']";
        await service.ComparePopulationsAsync(MonitorTracesTestHandler.Subscription, "rg", "amw0",
            "critical-path", request, aggregation, cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(3, handler.SubmitCalls);
        using var discovery = JsonDocument.Parse(handler.SubmitBodies[0]);
        Assert.Equal("Traces | where duration_nano > 10 | where start_time_unix_nano > 1767311999999999999 and start_time_unix_nano < 1767315600000000000 | project trace_id $$ <<Traces | where ['span.name'] == 'GET'>>",
            discovery.RootElement.GetProperty("query").GetString());
        Assert.Equal(10000, discovery.RootElement.GetProperty("options").GetProperty("maxResults").GetInt32());
        using var body = JsonDocument.Parse(handler.SubmitBodies[2]);
        Assert.Equal(aggregation, body.RootElement.GetProperty("current").GetProperty("query").GetString());
        Assert.Equal(new string('0', 31) + "a", body.RootElement.GetProperty("current").GetProperty("traceId").GetString());
        Assert.Equal(new string('0', 31) + "b", body.RootElement.GetProperty("compareTo").GetProperty("traceId").GetString());
        Assert.False(body.RootElement.GetProperty("compareTo").TryGetProperty("query", out _));
    }

    [Fact]
    public async Task CriticalPath_DiscoveryDeduplicatesEquivalentGuidFormats()
    {
        using var current = Page($$"""{"value":[{"trace_id":"{{Id}}"},{"trace_id":"01234567-89ab-cdef-0123-456789abcdef"}]}""");
        using var comparison = Page($$"""{"value":[{"trace_id":"{{ComparisonId}}"}]}""");
        using var handler = new MonitorTracesTestHandler(1, false, resultPages: [current, comparison]);
        var (service, _) = MonitorTracesServiceTests.CreateService(handler);
        var request = Request(false);
        request.Current.Options = new() { MaxResults = 1 };

        await service.ComparePopulationsAsync(MonitorTracesTestHandler.Subscription, "rg", "amw0",
            "critical-path", request, "Traces", cancellationToken: TestContext.Current.CancellationToken);

        using var body = JsonDocument.Parse(handler.LastSubmitBody!);
        Assert.Equal(Id, body.RootElement.GetProperty("current").GetProperty("traceId").GetString());
    }

    [Fact]
    public async Task CriticalPath_DeduplicatesExplicitIdsBeforeSubmitting()
    {
        using var handler = new MonitorTracesTestHandler(1, false);
        var (service, _) = MonitorTracesServiceTests.CreateService(handler);
        var request = Request(true);
        request.Current.Options = new() { MaxResults = 2 };
        request.Current.TraceId = $"{Id},01234567-89ab-cdef-0123-456789abcdef,{ComparisonId},{ComparisonId.ToUpperInvariant()}";
        request.CompareTo.TraceId = $"{ComparisonId},{Id},{Id.ToUpperInvariant()}";

        await service.ComparePopulationsAsync(MonitorTracesTestHandler.Subscription, "rg", "amw0",
            "critical-path", request, "Traces", cancellationToken: TestContext.Current.CancellationToken);

        using var body = JsonDocument.Parse(handler.LastSubmitBody!);
        Assert.Equal($"{Id},{ComparisonId}", body.RootElement.GetProperty("current").GetProperty("traceId").GetString());
        Assert.Equal($"{ComparisonId},{Id}", body.RootElement.GetProperty("compareTo").GetProperty("traceId").GetString());
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(true, true)]
    [InlineData(false, false)]
    [InlineData(false, true)]
    public async Task CriticalPath_RejectsPopulationsAboveRequestedLimit(bool explicitIds, bool compareTo)
    {
        string oneId = $$"""{"value":[{"trace_id":"{{Id}}"}]}""";
        string twoIds = $$"""{"value":[{"trace_id":"{{Id}}"},{"trace_id":"{{ComparisonId}}"}]}""";
        using var current = Page(compareTo ? oneId : twoIds);
        using var comparison = Page(twoIds);
        using var handler = new MonitorTracesTestHandler(1, false, resultPages: [current, comparison]);
        var (service, _) = MonitorTracesServiceTests.CreateService(handler);
        var request = Request(explicitIds);
        request.Current.Options = new() { MaxResults = 1 };
        if (explicitIds)
        {
            (compareTo ? request.CompareTo : request.Current).TraceId = $"{Id},{ComparisonId}";
        }

        var error = await Assert.ThrowsAsync<InvalidOperationException>(() => service.ComparePopulationsAsync(
            MonitorTracesTestHandler.Subscription, "rg", "amw0", "critical-path", request, "Traces",
            cancellationToken: TestContext.Current.CancellationToken));

        Assert.Contains("--max-results", error.Message, StringComparison.Ordinal);
        Assert.Equal(explicitIds ? 0 : compareTo ? 2 : 1, handler.SubmitCalls);
    }

    [Theory]
    [InlineData("""{"value":[{"trace_id":"a"}],"isPartialResult":true}""")]
    [InlineData("""{"value":[{"trace_id":"a"}],"metadata":{"isSystemLimitHit":true}}""")]
    [InlineData("""{"value":[{"trace_id":"a"}],"metadata":{"hasLimitReached":true}}""")]
    [InlineData("""{"value":[]}""")]
    [InlineData("""{"value":[{"trace_id":"invalid"}]}""")]
    public async Task CriticalPath_RejectsIncompleteOrInvalidDiscoveryBeforeComparison(string page)
    {
        using var response = Page(page);
        using var nextPopulation = Page("""{"value":[{"trace_id":"b"}]}""");
        using var handler = new MonitorTracesTestHandler(1, false, resultPages: [response, nextPopulation]);
        var (service, _) = MonitorTracesServiceTests.CreateService(handler);
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.ComparePopulationsAsync(
            MonitorTracesTestHandler.Subscription, "rg", "amw0", "critical-path", Request(false), "Traces | summarize count=count() by ['span.name']",
            cancellationToken: TestContext.Current.CancellationToken));
        Assert.Equal(1, handler.SubmitCalls);
        Assert.Equal("/traces:query", handler.SubmitUris[0].AbsolutePath);
    }

    [Fact]
    public async Task CriticalPath_DiscoveryPreservesNestedPartialFlagsAcrossPages()
    {
        using var firstPage = Page("""{"value":[{"trace_id":"a"}],"nextLink":"more","metadata":{"hasLimitReached":true}}""");
        using var finalPage = Page("""{"value":[{"trace_id":"b"}],"metadata":{"hasLimitReached":false}}""");
        using var handler = new MonitorTracesTestHandler(1, false, resultPages: [firstPage, finalPage]);
        var (service, _) = MonitorTracesServiceTests.CreateService(handler);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.ComparePopulationsAsync(
            MonitorTracesTestHandler.Subscription, "rg", "amw0", "critical-path", Request(false), "Traces",
            cancellationToken: TestContext.Current.CancellationToken));

        Assert.Single(handler.SubmitUris);
        Assert.Equal("/traces:query", handler.SubmitUris[0].AbsolutePath);
    }

    [Theory]
    [InlineData(1001, true)]
    [InlineData(10000, false)]
    public async Task CriticalPath_RejectsOversizedOrCappedDiscovery(int rowCount, bool distinct)
    {
        string rows = string.Join(",", Enumerable.Range(0, rowCount).Select(index =>
            $$"""{"trace_id":"{{(distinct ? index : 0).ToString("x32", System.Globalization.CultureInfo.InvariantCulture)}}" }"""));
        using var page = Page($$"""{"value":[{{rows}}]}""");
        using var handler = new MonitorTracesTestHandler(1, false, resultPages: [page]);
        var (service, _) = MonitorTracesServiceTests.CreateService(handler);
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.ComparePopulationsAsync(
            MonitorTracesTestHandler.Subscription, "rg", "amw0", "critical-path", Request(false), "Traces",
            cancellationToken: TestContext.Current.CancellationToken));
        Assert.Single(handler.SubmitUris);
        Assert.Equal("/traces:query", handler.SubmitUris[0].AbsolutePath);
    }

    private static HttpResponseMessage Page(string body) => new(HttpStatusCode.OK) { Content = new StringContent(body) };

    [Theory]
    [InlineData("Traces | where root_span_name == '$$'", "Traces | where root_span_name == '$$' | project trace_id")]
    [InlineData("Traces | where root_span_name == 'it''s|POST$$' $$ <<Traces | where ['span.name'] == @'it''s|POST$$'>>",
        "Traces | where root_span_name == 'it''s|POST$$' | project trace_id $$ <<Traces | where ['span.name'] == @'it''s|POST$$'>>")]
    [InlineData("Traces $$ <<Traces | where ['service.name'] == 'gateway' $$ Traces | where ['service.name'] == 'backend'>>",
        "Traces | project trace_id $$ <<Traces | where ['service.name'] == 'gateway' $$ Traces | where ['service.name'] == 'backend'>>")]
    [InlineData("T | where duration_nano > 10 $$ << T | where ['service.name'] == 'gateway' $$ T | where ['service.name'] == 'backend' >>",
        "T | where duration_nano > 10 | project trace_id $$ << T | where ['service.name'] == 'gateway' $$ T | where ['service.name'] == 'backend' >>")]
    [InlineData("Traces | where start_time_unix_nano > 0 $$ <<Traces | where ['span.name'] == 'GET'>>",
        "Traces | where start_time_unix_nano > 0 | project trace_id $$ <<Traces | where ['span.name'] == 'GET'>>")]
    public async Task CriticalPath_DiscoveryPreservesQuotedSeparatorsForBothPopulations(string query, string expected)
    {
        using var current = Page("""{"value":[{"trace_id":"a"}]}""");
        using var comparison = Page("""{"value":[{"trace_id":"b"}]}""");
        using var handler = new MonitorTracesTestHandler(1, false, resultPages: [current, comparison]);
        var (service, _) = MonitorTracesServiceTests.CreateService(handler);
        var request = new TraceComparisonRequest
        {
            Current = new()
            {
                Query = query,
                TimeRange = new() { StartTime = "2026-01-02T00:00:00.1Z", EndTime = "2026-01-02T00:01:00.9Z" },
                Options = new() { MaxResults = 10000 },
            },
            CompareTo = new()
            {
                Query = query,
                TimeRange = new() { StartTime = "2026-01-01T00:00:00.1Z", EndTime = "2026-01-01T00:01:00.9Z" },
            },
        };
        await service.ComparePopulationsAsync(MonitorTracesTestHandler.Subscription, "rg", "amw0",
            "critical-path", request, "Traces | summarize count=count() by ['span.name']",
            cancellationToken: TestContext.Current.CancellationToken);
        Assert.Equal(3, handler.SubmitBodies.Count);
        for (int index = 0; index < 2; index++)
        {
            using var body = JsonDocument.Parse(handler.SubmitBodies[index]);
            var expectedWindow = index == 0 ? request.Current.TimeRange : request.CompareTo.TimeRange;
            string windowFilter = index == 0
                ? " | where start_time_unix_nano > 1767312000099999999 and start_time_unix_nano < 1767312060900000000"
                : " | where start_time_unix_nano > 1767225600099999999 and start_time_unix_nano < 1767225660900000000";
            Assert.Equal(expected.Replace(" | project trace_id", windowFilter + " | project trace_id", StringComparison.Ordinal),
                body.RootElement.GetProperty("query").GetString());
            Assert.Equal(expectedWindow!.StartTime, body.RootElement.GetProperty("timeRange").GetProperty("startTime").GetString());
            Assert.Equal(expectedWindow.EndTime, body.RootElement.GetProperty("timeRange").GetProperty("endTime").GetString());
        }
    }

    private static TraceComparisonRequest Request(bool criticalPath) => new()
    {
        Current = new()
        {
            Query = criticalPath ? null : "Traces",
            Language = "KQL",
            TraceId = criticalPath ? Id : null,
            TimeRange = new() { StartTime = "2026-01-02T00:00:00Z", EndTime = "2026-01-02T01:00:00Z" },
            Options = new() { MaxResults = 10000 },
        },
        CompareTo = new()
        {
            Query = criticalPath ? null : "Traces | where error_rate > 0",
            TraceId = criticalPath ? ComparisonId : null,
            TimeRange = new() { StartTime = "2026-01-01T00:00:00Z", EndTime = "2026-01-01T01:00:00Z" },
        },
    };
}
