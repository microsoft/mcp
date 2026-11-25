// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Microsoft.Mcp.Tests.Client;
using Microsoft.Mcp.Tests.Client.Helpers;
using Microsoft.Mcp.Tests.Generated.Models;
using Xunit;

namespace Azure.Mcp.Tools.Adme.Tests.Recorded;

/// <summary>Tests ADME search operations through recorded MCP interactions.</summary>
public sealed class SearchRecordedTests(
    ITestOutputHelper output,
    TestProxyFixture fixture,
    LiveServerFixture liveServerFixture,
    RecordSeeder seeder)
    : AdmeRecordedTestsBase(output, fixture, liveServerFixture), IClassFixture<RecordSeeder>
{
    private const string SearchTool = "adme_search";

    private List<GeneralRegexSanitizer>? _generalRegexSanitizers;

    public override List<GeneralRegexSanitizer> GeneralRegexSanitizers =>
        _generalRegexSanitizers ??=
        [
            .. base.GeneralRegexSanitizers,
            new(new()
            {
                Regex = seeder.DataPartition,
                Value = "recording-partition",
            }),
            new(new()
            {
                Regex = seeder.Marker,
                Value = "recording",
            }),
        ];

    [Fact]
    public async Task Server_exposes_search_tool_over_mcp()
    {
        var toolNames = await ListToolNamesAsync();

        Assert.Contains(SearchTool, toolNames);
    }

    [Fact]
    public async Task Search_returns_projected_records_for_well_kind()
    {
        var arguments = CreateArguments();
        arguments["kind"] = new[] { TestConstants.WellKind };
        arguments["query"] = "id:*";
        arguments["limit"] = 2;
        arguments["returned-fields"] = new[] { "id", "kind" };

        var result = await CallToolResultsAsync(SearchTool, arguments);
        var records = result.GetProperty("results");

        Assert.Equal(JsonValueKind.Array, records.ValueKind);
        Assert.True(records.GetArrayLength() <= 2);
        Assert.All(records.EnumerateArray(), record =>
        {
            Assert.True(record.TryGetProperty("id", out _));
            Assert.Equal(TestConstants.WellKind, record.GetProperty("kind").GetString());
        });
    }

    [Fact]
    public async Task Search_matches_trailing_field_value_wildcard()
    {
        var arguments = CreateArguments();
        arguments["kind"] = new[] { TestConstants.WellKind };
        arguments["query"] = $"data.FacilityID:{seeder.Marker}*";
        arguments["limit"] = 10;
        arguments["returned-fields"] = new[] { "id", "kind", "data.FacilityID" };

        var result = await CallToolResultsAsync(SearchTool, arguments);
        var records = result.GetProperty("results");

        Assert.NotEmpty(records.EnumerateArray());
        Assert.All(records.EnumerateArray(), record =>
        {
            Assert.Equal(TestConstants.WellKind, record.GetProperty("kind").GetString());
            Assert.Contains(
                seeder.Marker,
                record.GetProperty("data").GetProperty("FacilityID").GetString(),
                StringComparison.OrdinalIgnoreCase);
        });
    }

    [Fact]
    public async Task Search_rejects_leading_field_value_wildcard()
    {
        var arguments = CreateArguments();
        arguments["kind"] = new[] { TestConstants.WellKind };
        arguments["query"] = $"data.FacilityID:*{seeder.Marker}";

        Assert.True(await CallToolReturnsErrorAsync(SearchTool, arguments));
    }

    [Fact]
    public async Task Search_continues_to_a_different_page()
    {
        var firstPageArguments = CreateArguments();
        firstPageArguments["kind"] = new[] { TestConstants.WellKind };
        firstPageArguments["cursor-pagination-mode"] = true;
        firstPageArguments["limit"] = 1;
        firstPageArguments["returned-fields"] = new[] { "id" };

        var firstResponse = await CallToolResponseAsync(SearchTool, firstPageArguments);
        var firstPage = firstResponse.GetProperty("result");
        var cursor = firstResponse.GetProperty("nextCursor").GetString();
        Assert.False(string.IsNullOrEmpty(cursor));
        Assert.False(firstPage.TryGetProperty("cursor", out _));

        var nextPageArguments = new Dictionary<string, object?>(firstPageArguments)
        {
            ["cursor"] = cursor,
        };
        var nextResponse = await CallToolResponseAsync(SearchTool, nextPageArguments);
        var nextPage = nextResponse.GetProperty("result");
        Assert.False(nextPage.TryGetProperty("cursor", out _));

        Assert.NotEqual(
            firstPage.GetProperty("results").EnumerateArray().Single().GetProperty("id").GetString(),
            nextPage.GetProperty("results").EnumerateArray().Single().GetProperty("id").GetString());
    }

    [Fact]
    public async Task Search_empty_kind_returns_error()
    {
        var arguments = CreateArguments();
        arguments["kind"] = Array.Empty<string>();

        Assert.True(await CallToolReturnsErrorAsync(SearchTool, arguments));
    }

    [Fact]
    public async Task Search_rejects_invalid_paging()
    {
        var arguments = CreateArguments();
        arguments["kind"] = new[] { TestConstants.WellKind };
        arguments["offset"] = 9999;
        arguments["limit"] = 2;

        Assert.True(await CallToolReturnsErrorAsync(SearchTool, arguments));
    }
}
