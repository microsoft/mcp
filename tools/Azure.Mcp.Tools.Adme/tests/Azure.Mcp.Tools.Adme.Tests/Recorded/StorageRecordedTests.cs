// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Microsoft.Mcp.Tests.Client;
using Microsoft.Mcp.Tests.Client.Helpers;
using Microsoft.Mcp.Tests.Generated.Models;
using Xunit;

namespace Azure.Mcp.Tools.Adme.Tests.Recorded;

/// <summary>Tests ADME storage operations through recorded MCP interactions.</summary>
public sealed class StorageRecordedTests(
    ITestOutputHelper output,
    TestProxyFixture fixture,
    LiveServerFixture liveServerFixture,
    RecordSeeder seeder)
    : AdmeRecordedTestsBase(output, fixture, liveServerFixture), IClassFixture<RecordSeeder>
{
    private const string RecordFetchTool = "adme_storage_record_fetch";
    private const string RecordGetTool = "adme_storage_record_get";
    private const string RecordListTool = "adme_storage_record_list";
    private const string RecordVersionListTool = "adme_storage_record_version_list";
    private const string WellKind = "osdu:wks:master-data--Well:1.0.0";

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
    public async Task Server_exposes_storage_tools_over_mcp()
    {
        var toolNames = await ListToolNamesAsync();

        Assert.Contains(RecordFetchTool, toolNames);
        Assert.Contains(RecordGetTool, toolNames);
        Assert.Contains(RecordListTool, toolNames);
        Assert.Contains(RecordVersionListTool, toolNames);
    }

    [Fact]
    public async Task RecordList_returns_ids_for_well_kind()
    {
        var arguments = CreateArguments();
        arguments["kind"] = WellKind;
        arguments["limit"] = 5;

        var result = await CallToolResultsAsync(RecordListTool, arguments);
        var results = result.GetProperty("results");

        Assert.Equal(JsonValueKind.Array, results.ValueKind);
        Assert.True(results.GetArrayLength() <= 5);
        Assert.All(results.EnumerateArray(), id =>
            Assert.Contains("master-data--Well:", id.GetString()));
    }

    [Fact]
    public async Task RecordList_paginates_with_cursor()
    {
        var page1Arguments = CreateArguments();
        page1Arguments["kind"] = WellKind;
        page1Arguments["limit"] = 1;

        var page1 = await CallToolResultsAsync(RecordListTool, page1Arguments);
        var cursor = page1.GetProperty("cursor").GetString();
        Assert.False(string.IsNullOrEmpty(cursor));

        var page2Arguments = new Dictionary<string, object?>(page1Arguments) { ["cursor"] = cursor };
        var page2 = await CallToolResultsAsync(RecordListTool, page2Arguments);

        Assert.NotEqual(
            page1.GetProperty("results").EnumerateArray().Single().GetString(),
            page2.GetProperty("results").EnumerateArray().Single().GetString());
    }

    [Fact]
    public async Task RecordList_empty_kind_returns_error()
    {
        var arguments = CreateArguments();
        arguments["kind"] = string.Empty;

        Assert.True(await CallToolReturnsErrorAsync(RecordListTool, arguments));
    }

    [Fact]
    public async Task RecordGet_returns_seeded_record()
    {
        var arguments = CreateArguments();
        arguments["id"] = seeder.FirstId;

        var result = await CallToolResultsAsync(RecordGetTool, arguments);

        Assert.Equal(seeder.FirstId, result.GetProperty("id").GetString());
        Assert.True(result.TryGetProperty("data", out _));
    }

    [Fact]
    public async Task RecordGet_with_attributes_returns_only_projected_field()
    {
        var arguments = CreateArguments();
        arguments["id"] = seeder.FirstId;
        arguments["attributes"] = new[] { "data.Name" };

        var result = await CallToolResultsAsync(RecordGetTool, arguments);
        var data = result.GetProperty("data");

        Assert.Equal(new[] { "Name" }, data.EnumerateObject().Select(property => property.Name).ToArray());
    }

    [Fact]
    public async Task RecordGet_with_version_returns_that_version()
    {
        var versionArguments = CreateArguments();
        versionArguments["id"] = seeder.FirstId;
        var versions = await CallToolResultsAsync(RecordVersionListTool, versionArguments);
        var version = versions.GetProperty("versions").EnumerateArray().First().GetInt64();

        var arguments = CreateArguments();
        arguments["id"] = seeder.FirstId;
        arguments["version"] = version;
        var result = await CallToolResultsAsync(RecordGetTool, arguments);

        Assert.Equal(seeder.FirstId, result.GetProperty("id").GetString());
        Assert.Equal(version, result.GetProperty("version").GetInt64());
    }

    [Fact]
    public async Task RecordGet_nonexistent_id_returns_error()
    {
        var arguments = CreateArguments();
        arguments["id"] = $"{seeder.DataPartition}:master-data--Well:does-not-exist";

        Assert.True(await CallToolReturnsErrorAsync(RecordGetTool, arguments));
    }

    [Fact]
    public async Task RecordGet_empty_id_returns_error()
    {
        var arguments = CreateArguments();
        arguments["id"] = string.Empty;

        Assert.True(await CallToolReturnsErrorAsync(RecordGetTool, arguments));
    }

    [Fact]
    public async Task RecordVersionList_returns_versions_for_seeded_record()
    {
        var arguments = CreateArguments();
        arguments["id"] = seeder.FirstId;

        var result = await CallToolResultsAsync(RecordVersionListTool, arguments);

        Assert.Equal(seeder.FirstId, result.GetProperty("recordId").GetString());
        Assert.NotEmpty(result.GetProperty("versions").EnumerateArray());
    }

    [Fact]
    public async Task RecordVersionList_empty_id_returns_error()
    {
        var arguments = CreateArguments();
        arguments["id"] = string.Empty;

        Assert.True(await CallToolReturnsErrorAsync(RecordVersionListTool, arguments));
    }

    [Fact]
    public async Task RecordFetch_returns_all_seeded_records()
    {
        var arguments = CreateArguments();
        arguments["ids"] = seeder.Ids.ToArray();

        var result = await CallToolResultsAsync(RecordFetchTool, arguments);
        var returnedIds = result.GetProperty("records").EnumerateArray()
            .Select(record => record.GetProperty("id").GetString());

        Assert.Equal(
            seeder.Ids.OrderBy(id => id, StringComparer.Ordinal),
            returnedIds.OrderBy(id => id, StringComparer.Ordinal));
    }

    [Fact]
    public async Task RecordFetch_with_attributes_returns_only_projected_field()
    {
        var arguments = CreateArguments();
        arguments["ids"] = new[] { seeder.FirstId };
        arguments["attributes"] = new[] { "data.Name" };

        var result = await CallToolResultsAsync(RecordFetchTool, arguments);
        var data = result.GetProperty("records").EnumerateArray().First().GetProperty("data");

        Assert.Equal(new[] { "Name" }, data.EnumerateObject().Select(property => property.Name).ToArray());
    }

    [Fact]
    public async Task RecordFetch_with_empty_attributes_returns_error()
    {
        var arguments = CreateArguments();
        arguments["ids"] = new[] { seeder.FirstId };
        arguments["attributes"] = new[] { "" };

        Assert.True(await CallToolReturnsErrorAsync(RecordFetchTool, arguments));
    }

    [Fact]
    public async Task RecordFetch_with_frame_of_reference_reports_conversion_status()
    {
        var arguments = CreateArguments();
        arguments["ids"] = new[] { seeder.FirstId };
        arguments["frame-of-reference"] = true;

        var result = await CallToolResultsAsync(RecordFetchTool, arguments);

        Assert.Equal(1, result.GetProperty("records").GetArrayLength());
        var status = result.GetProperty("conversionStatuses").EnumerateArray().Single();
        Assert.Equal(seeder.FirstId, status.GetProperty("id").GetString());
        Assert.Equal("NO_FRAME_OF_REFERENCE", status.GetProperty("status").GetString());
    }

    [Fact]
    public async Task RecordFetch_empty_ids_returns_error()
    {
        var arguments = CreateArguments();
        arguments["ids"] = Array.Empty<string>();

        Assert.True(await CallToolReturnsErrorAsync(RecordFetchTool, arguments));
    }
}
