// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text;
using System.Text.Json;
using Azure.Core;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Core.Services.Caching;
using Azure.Mcp.Tools.Kusto.Services;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.Kusto.UnitTests;

public sealed class KustoServiceTests
{
    [Fact]
    public async Task QueryItemsWithStatisticsAsync_V2PrimaryResult_ExtractsItems()
    {
        var service = CreateService(
            """
            [
              {"FrameType":"DataSetHeader","IsProgressive":false,"Version":"v2.0"},
              {"FrameType":"DataTable","TableId":5,"TableKind":"QueryProperties","TableName":"@ExtendedProperties","Columns":[],"Rows":[]},
              {"FrameType":"DataTable","TableId":0,"TableKind":"PrimaryResult","TableName":"PrimaryResult","Columns":[{"ColumnName":"name","ColumnType":"string"},{"ColumnName":"count","ColumnType":"int"}],"Rows":[["alpha",1],["beta",2]]},
              {"FrameType":"DataSetCompletion","HasErrors":false,"Cancelled":false}
            ]
            """,
            out var handler);

        var result = await service.QueryItemsWithStatisticsAsync(
            "https://test.kusto.windows.net",
            "db1",
            "StormEvents | take 2",
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal("/v2/rest/query", handler.LastRequest!.RequestUri!.AbsolutePath);
        Assert.Equal(3, result.Items.Count);
        Assert.Equal("string", result.Items[0].GetProperty("name").GetString());
        Assert.Equal("alpha", result.Items[1][0].GetString());
        Assert.Equal(2, result.Items[2][1].GetInt32());
    }

    [Fact]
    public async Task QueryItemsWithStatisticsAsync_DataSetCompletionHasErrors_Throws()
    {
        var service = CreateService(
            """
            [
              {"FrameType":"DataSetHeader","IsProgressive":false,"Version":"v2.0"},
              {"FrameType":"DataTable","TableId":0,"TableKind":"PrimaryResult","TableName":"PrimaryResult","Columns":[{"ColumnName":"name","ColumnType":"string"}],"Rows":[["alpha"]]},
              {"FrameType":"DataSetCompletion","HasErrors":true,"Cancelled":false,"OneApiErrors":{"code":"BadRequest","message":"boom"}}
            ]
            """,
            out _);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => service.QueryItemsWithStatisticsAsync(
            "https://test.kusto.windows.net",
            "db1",
            "StormEvents | take 1",
            cancellationToken: TestContext.Current.CancellationToken));

        Assert.Contains("BadRequest", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task QueryItemsWithStatisticsAsync_MissingPrimaryResult_Throws()
    {
        var service = CreateService(
            """
            [
              {"FrameType":"DataSetHeader","IsProgressive":false,"Version":"v2.0"},
              {"FrameType":"DataTable","TableId":1,"TableKind":"QueryCompletionInformation","TableName":"QueryStatus","Columns":[],"Rows":[]},
              {"FrameType":"DataSetCompletion","HasErrors":false,"Cancelled":false}
            ]
            """,
            out _);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => service.QueryItemsWithStatisticsAsync(
            "https://test.kusto.windows.net",
            "db1",
            "StormEvents | take 1",
            cancellationToken: TestContext.Current.CancellationToken));

        Assert.Contains("PrimaryResult", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task QueryItemsWithStatisticsAsync_ShowStatsFalse_ReturnsNullStatistics()
    {
        var service = CreateService(CreateStatsResponse(), out _);

        var result = await service.QueryItemsWithStatisticsAsync(
            "https://test.kusto.windows.net",
            "db1",
            "StormEvents | take 1",
            showStats: false,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Null(result.Statistics);
        Assert.Equal(2, result.Items.Count);
    }

    [Fact]
    public async Task QueryItemsWithStatisticsAsync_ShowStatsTrue_ExtractsStatistics()
    {
        var service = CreateService(CreateStatsResponse(), out _);

        var result = await service.QueryItemsWithStatisticsAsync(
            "https://test.kusto.windows.net",
            "db1",
            "StormEvents | take 1",
            showStats: true,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(result.Statistics);
        Assert.Equal(1.234, result.Statistics!.Value.GetProperty("execution_time_sec").GetDouble());
        Assert.Equal(45.2, result.Statistics!.Value.GetProperty("memory_peak_per_node_mb").GetDouble());
        Assert.Equal(5.2, result.Statistics!.Value.GetProperty("network").GetProperty("cross_cluster_mb").GetDouble());
    }

    [Fact]
    public async Task QueryItemsWithStatisticsAsync_ShowStatsTrue_MissingStatsFrame_ReturnsNullStatistics()
    {
        var service = CreateService(
            """
            [
              {"FrameType":"DataSetHeader","IsProgressive":false,"Version":"v2.0"},
              {"FrameType":"DataTable","TableId":0,"TableKind":"PrimaryResult","TableName":"PrimaryResult","Columns":[{"ColumnName":"name","ColumnType":"string"}],"Rows":[["alpha"]]},
              {"FrameType":"DataSetCompletion","HasErrors":false,"Cancelled":false}
            ]
            """,
            out _);

        var result = await service.QueryItemsWithStatisticsAsync(
            "https://test.kusto.windows.net",
            "db1",
            "StormEvents | take 1",
            showStats: true,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Null(result.Statistics);
    }

    [Fact]
    public async Task QueryItemsWithStatisticsAsync_ShowStatsTrue_ParsesCrossClusterBreakdown()
    {
        var service = CreateService(CreateStatsResponse(), out _);

        var result = await service.QueryItemsWithStatisticsAsync(
            "https://test.kusto.windows.net",
            "db1",
            "StormEvents | take 1",
            showStats: true,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(result.Statistics);
        var cluster = result.Statistics!.Value
            .GetProperty("cross_cluster_breakdown")
            .GetProperty("clustername.region.kusto.windows.net");
        Assert.Equal("00:00:00.8", cluster.GetProperty("cpu_total").GetString());
        Assert.Equal(22.1, cluster.GetProperty("memory_peak_mb").GetDouble());
        Assert.Equal(50.0, cluster.GetProperty("cache_hit_mb").GetDouble());
        Assert.Equal(1.0, cluster.GetProperty("cache_miss_mb").GetDouble());
    }

    private static KustoService CreateService(string responsePayload, out MockHttpMessageHandler handler)
    {
        var subscriptionService = Substitute.For<ISubscriptionService>();
        var tenantService = Substitute.For<ITenantService>();
        var cacheService = Substitute.For<ICacheService>();
        var httpClientFactory = Substitute.For<IHttpClientFactory>();
        var logger = Substitute.For<ILogger<KustoService>>();

        var tokenCredential = Substitute.For<TokenCredential>();
        tokenCredential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
            .Returns(new ValueTask<AccessToken>(new AccessToken("noop-token", DateTimeOffset.UtcNow.AddHours(1))));
        tenantService.GetTokenCredentialAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<TokenCredential>(tokenCredential));

        cacheService.GetAsync<KustoClient>(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<TimeSpan?>(),
            Arg.Any<CancellationToken>())
            .Returns(new ValueTask<KustoClient?>((KustoClient?)null));

        handler = new MockHttpMessageHandler(responsePayload);
        httpClientFactory.CreateClient(Arg.Any<string>())
            .Returns(new HttpClient(handler));

        return new KustoService(subscriptionService, tenantService, cacheService, httpClientFactory, logger);
    }

    private static string CreateStatsResponse()
    {
        var statsJson = """
            {
              "ExecutionTime": 1.234,
              "resource_usage": {
                "cpu": {
                  "total cpu": "00:00:01.5",
                  "breakdown": {
                    "query execution": "00:00:01.2",
                    "query planning": "00:00:00.3"
                  }
                },
                "memory": { "peak_per_node": 47395635 },
                "cache": { "shards": { "hot": { "hitbytes": 126353408, "missbytes": 3355443 } } },
                "network": { "cross_cluster_total_bytes": 5452595, "inter_cluster_total_bytes": 0 }
              },
              "input_dataset_statistics": {
                "extents": { "scanned": 42, "total": 1000 },
                "rows": { "scanned": 50000, "total": 1000000 }
              },
              "dataset_statistics": [{ "table_row_count": 150, "table_size": 12800 }],
              "cross_cluster_resource_usage": {
                "https://clustername.region.kusto.windows.net/": {
                  "cpu": { "total cpu": "00:00:00.8" },
                  "memory": { "peak_per_node": 23173530 },
                  "cache": { "shards": { "hot": { "hitbytes": 52428800, "missbytes": 1048576 } } }
                }
              }
            }
            """;

        return """
            [
              {"FrameType":"DataSetHeader","IsProgressive":false,"Version":"v2.0"},
              {"FrameType":"DataTable","TableId":0,"TableKind":"PrimaryResult","TableName":"PrimaryResult","Columns":[{"ColumnName":"name","ColumnType":"string"}],"Rows":[["alpha"]]},
              {"FrameType":"DataTable","TableId":1,"TableKind":"QueryCompletionInformation","TableName":"QueryStatus","Columns":[{"ColumnName":"Timestamp","ColumnType":"datetime"},{"ColumnName":"Severity","ColumnType":"int"},{"ColumnName":"SeverityName","ColumnType":"string"},{"ColumnName":"StatusCode","ColumnType":"int"},{"ColumnName":"StatusDescription","ColumnType":"string"},{"ColumnName":"Count","ColumnType":"int"},{"ColumnName":"RequestId","ColumnType":"guid"},{"ColumnName":"ActivityId","ColumnType":"guid"},{"ColumnName":"SubActivityId","ColumnType":"guid"},{"ColumnName":"ClientActivityId","ColumnType":"string"}],"Rows":[["2024-01-01T00:00:00Z",4,"Info",0,"Query completed",1,"...","...","...","..."],["2024-01-01T00:00:00Z",4,"Stats",0,__STATUS_DESCRIPTION__,1,"...","...","...","..."]]},
              {"FrameType":"DataSetCompletion","HasErrors":false,"Cancelled":false}
            ]
            """.Replace("__STATUS_DESCRIPTION__", JsonSerializer.Serialize(statsJson), StringComparison.Ordinal);
    }

    private sealed class MockHttpMessageHandler(string responsePayload) : HttpMessageHandler
    {
        public HttpRequestMessage? LastRequest { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastRequest = request;
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responsePayload, Encoding.UTF8, "application/json")
            };
            return Task.FromResult(response);
        }
    }
}
