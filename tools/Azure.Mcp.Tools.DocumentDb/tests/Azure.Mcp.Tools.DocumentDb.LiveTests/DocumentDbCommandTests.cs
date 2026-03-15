// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Mcp.Tests;
using Microsoft.Mcp.Tests.Client;
using Microsoft.Mcp.Tests.Client.Helpers;
using Microsoft.Mcp.Tests.Generated.Models;
using Xunit;

namespace Azure.Mcp.Tools.DocumentDb.LiveTests;

public class DocumentDbCommandTests(ITestOutputHelper output, TestProxyFixture fixture, LiveServerFixture serverFixture)
    : RecordedCommandTestsBase(output, fixture, serverFixture)
{
    private string? connectionString;
    private string sanitizerConnectionString = "mongodb://Sanitized";

    protected override RecordingOptions? RecordingOptions => new()
    {
        HandleRedirects = false
    };

    public override CustomDefaultMatcher? TestMatcher => new()
    {
        IgnoredHeaders = "x-ms-activity-id,x-ms-request-id,x-ms-session-token"
    };

    public override List<string> DisabledDefaultSanitizers => [.. base.DisabledDefaultSanitizers, "AZSDK3493"];

    public override List<GeneralRegexSanitizer> GeneralRegexSanitizers =>
    [
        ..base.GeneralRegexSanitizers,
        new GeneralRegexSanitizer(new GeneralRegexSanitizerBody
        {
            Regex = Regex.Escape(sanitizerConnectionString),
            Value = "mongodb://Sanitized"
        })
    ];

    public override async ValueTask InitializeAsync()
    {
        await LoadSettingsAsync();

        if (Settings.DeploymentOutputs.TryGetValue("DOCUMENTDB_CONNECTION_STRING", out connectionString) &&
            !string.IsNullOrEmpty(connectionString))
        {
            sanitizerConnectionString = connectionString;
        }

        await base.InitializeAsync();
    }

    private string GetConnectionString()
    {
        Assert.SkipWhen(TestMode == Microsoft.Mcp.Tests.Helpers.TestMode.Playback,
            "DocumentDb live tests require a real MongoDB connection string and do not support playback with a sanitized placeholder");

        Assert.SkipWhen(string.IsNullOrEmpty(connectionString),
            "DocumentDb connection string not configured in deployment outputs for live testing");

        return connectionString!;
    }

    [Fact]
    public async Task Should_list_indexes_with_connection_string()
    {
        var documentDbConnectionString = GetConnectionString();

        var result = await CallToolAsync(
            "documentdb_index_list_indexes",
            new()
            {
                { "connection-string", documentDbConnectionString },
                { "db-name", "test" },
                { "collection-name", "items" }
            });

        var indexesArray = result.AssertProperty("indexes");
        Assert.Equal(JsonValueKind.Array, indexesArray.ValueKind);
        Assert.NotEmpty(indexesArray.EnumerateArray());
    }

    [Fact]
    public async Task Should_create_and_drop_index_with_connection_string()
    {
        const string indexName = "value_1_mcp";
        var documentDbConnectionString = GetConnectionString();

        var createResult = await CallToolAsync(
            "documentdb_index_create_index",
            new()
            {
                { "connection-string", documentDbConnectionString },
                { "db-name", "test" },
                { "collection-name", "items" },
                { "keys", "{\"value\":1}" },
                { "options", $"{{\"name\":\"{indexName}\"}}" }
            });

        Assert.Equal(indexName, createResult.AssertProperty("index_name").GetString());

        var listResult = await CallToolAsync(
            "documentdb_index_list_indexes",
            new()
            {
                { "connection-string", documentDbConnectionString },
                { "db-name", "test" },
                { "collection-name", "items" }
            });

        Assert.Contains(listResult.AssertProperty("indexes").EnumerateArray(), element =>
            element.GetString()?.Contains(indexName, StringComparison.Ordinal) == true);

        var dropResult = await CallToolAsync(
            "documentdb_index_drop_index",
            new()
            {
                { "connection-string", documentDbConnectionString },
                { "db-name", "test" },
                { "collection-name", "items" },
                { "index-name", indexName }
            });

        Assert.Equal(indexName, dropResult.AssertProperty("index_name").GetString());
    }

    [Fact]
    public async Task Should_get_index_stats_with_connection_string()
    {
        const string indexName = "category_1_mcp";
        var documentDbConnectionString = GetConnectionString();

        await CallToolAsync(
            "documentdb_index_create_index",
            new()
            {
                { "connection-string", documentDbConnectionString },
                { "db-name", "test" },
                { "collection-name", "items" },
                { "keys", "{\"category\":1}" },
                { "options", $"{{\"name\":\"{indexName}\"}}" }
            });

        var statsResult = await CallToolAsync(
            "documentdb_index_index_stats",
            new()
            {
                { "connection-string", documentDbConnectionString },
                { "db-name", "test" },
                { "collection-name", "items" }
            });

        var stats = statsResult.AssertProperty("stats");
        Assert.Equal(JsonValueKind.Array, stats.ValueKind);
        Assert.True(stats.EnumerateArray().Any());

        await CallToolAsync(
            "documentdb_index_drop_index",
            new()
            {
                { "connection-string", documentDbConnectionString },
                { "db-name", "test" },
                { "collection-name", "items" },
                { "index-name", indexName }
            });
    }
}