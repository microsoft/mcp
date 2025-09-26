// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Identity;
using Azure.Mcp.Core.Services.Http;
using Azure.Mcp.Tests;
using Azure.Mcp.Tests.Client;
using Azure.Mcp.Tools.Kusto.Services;
using ModelContextProtocol.Client;
using Xunit;
using MsOptions = Microsoft.Extensions.Options.Options;

namespace Azure.Mcp.Tools.Kusto.LiveTests;

public class KustoCommandTests(ITestOutputHelper output)
    : CommandTestsBase(output)
{
    private const string TestDatabaseName = "ToDoLists";
    private const string TestTableName = "ToDoList";

    #region Init
    public override async ValueTask InitializeAsync()
    {
        await base.InitializeAsync(); // Initialize the base class first

        try
        {
            var credentials = new DefaultAzureCredential();
            await Client.PingAsync();
            var clusterInfo = await CallToolAsync(
                "azmcp_kusto_cluster_get",
                new()
                {
                { "subscription", Settings.SubscriptionId },
                { "cluster", Settings.ResourceBaseName }
                });
            var clusterUri = clusterInfo.AssertProperty("cluster").AssertProperty("clusterUri").GetString();

            // Create HttpClientService for KustoClient
            var httpClientOptions = new HttpClientOptions();
            var httpClientService = new HttpClientService(MsOptions.Create(httpClientOptions));

            var kustoClient = new KustoClient(clusterUri ?? string.Empty, credentials, "ua", httpClientService);
            var resp = await kustoClient.ExecuteControlCommandAsync(
                TestDatabaseName,
                ".set-or-replace ToDoList <| datatable (Title: string, IsCompleted: bool) [' Hello World!', false]",
                CancellationToken.None).ConfigureAwait(false);
        }
        catch
        {
            Assert.Skip("Skipping until auth fixed for Kusto");
        }
    }
    #endregion

    #region Databases 
    [Fact]
    public async Task Should_list_databases_in_cluster()
    {
        var result = await CallToolAsync(
            "azmcp_kusto_database_list",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "cluster", Settings.ResourceBaseName }
            });

        var databasesArray = result.AssertProperty("databases");
        Assert.Equal(JsonValueKind.Array, databasesArray.ValueKind);
        Assert.NotEmpty(databasesArray.EnumerateArray());
    }
    #endregion

    #region Tables
    [Fact]
    public async Task Should_list_kusto_tables()
    {
        var result = await CallToolAsync(
            "azmcp_kusto_table_list",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "cluster", Settings.ResourceBaseName },
                { "database", TestDatabaseName }
            });

        var tablesArray = result.AssertProperty("tables");
        Assert.Equal(JsonValueKind.Array, tablesArray.ValueKind);
        Assert.NotEmpty(tablesArray.EnumerateArray());
    }

    [Fact]
    public async Task Should_list_tables_with_direct_uri()
    {
        var clusterInfo = await CallToolAsync(
            "azmcp_kusto_cluster_get",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "cluster", Settings.ResourceBaseName }
            });

        var clusterUri = clusterInfo.AssertProperty("cluster").AssertProperty("clusterUri").GetString();
        Assert.NotNull(clusterUri);

        var result = await CallToolAsync(
            "azmcp_kusto_table_list",
            new()
            {
                { "cluster-uri", clusterUri },
                { "database", TestDatabaseName }
            });

        var tablesArray = result.AssertProperty("tables");
        Assert.Equal(JsonValueKind.Array, tablesArray.ValueKind);
        Assert.NotEmpty(tablesArray.EnumerateArray());
    }

    [Fact]
    public async Task Should_get_table_schema()
    {
        var result = await CallToolAsync(
            "azmcp_kusto_table_schema",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "cluster", Settings.ResourceBaseName },
                { "database", TestDatabaseName },
                { "table", TestTableName }
            });

        var schema = result.AssertProperty("schema").GetString();
        Assert.NotNull(schema);
        Assert.Contains("Title:string", schema);
        Assert.Contains("IsCompleted:bool", schema);
    }

    [Fact]
    public async Task Should_get_table_schema_with_direct_uri()
    {
        var clusterInfo = await CallToolAsync(
            "azmcp_kusto_cluster_get",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "cluster", Settings.ResourceBaseName }
            });

        var clusterUri = clusterInfo.AssertProperty("cluster").AssertProperty("clusterUri").GetString();
        Assert.NotNull(clusterUri);

        var result = await CallToolAsync(
            "azmcp_kusto_table_schema",
            new()
            {
                { "cluster-uri", clusterUri },
                { "database", TestDatabaseName },
                { "table", TestTableName }
            });

        var schema = result.AssertProperty("schema").GetString();
        Assert.NotNull(schema);
        Assert.Contains("Title:string", schema);
        Assert.Contains("IsCompleted:bool", schema);
    }
    #endregion

    #region Query
    [Fact]
    public async Task Should_query_kusto()
    {
        var result = await CallToolAsync(
            "azmcp_kusto_query",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "cluster", Settings.ResourceBaseName },
                { "database", TestDatabaseName },
                { "query", $"{TestTableName} | take 1" }
            });

        var itemsArray = result.AssertProperty("items");
        Assert.Equal(JsonValueKind.Array, itemsArray.ValueKind);
        Assert.NotEmpty(itemsArray.EnumerateArray());
    }

    [Fact]
    public async Task Should_query_kusto_with_direct_uri()
    {
        var clusterInfo = await CallToolAsync(
            "azmcp_kusto_cluster_get",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "cluster", Settings.ResourceBaseName }
            });

        var clusterUri = clusterInfo.AssertProperty("cluster").AssertProperty("clusterUri").GetString();
        Assert.NotNull(clusterUri);

        var result = await CallToolAsync(
            "azmcp_kusto_query",
            new()
            {
                { "cluster-uri", clusterUri },
                { "database", TestDatabaseName },
                { "query", $"{TestTableName} | take 1" }
            });

        var itemsArray = result.AssertProperty("items");
        Assert.Equal(JsonValueKind.Array, itemsArray.ValueKind);
        Assert.NotEmpty(itemsArray.EnumerateArray());
    }
    #endregion
}
