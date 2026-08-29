// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Microsoft.Mcp.Tests.Client;
using Microsoft.Mcp.Tests.Client.Helpers;
using Xunit;

namespace Azure.Mcp.Tools.Adme.Tests.Recorded;

/// <summary>Tests ADME schema operations through recorded MCP interactions.</summary>
public sealed class SchemaRecordedTests(
    ITestOutputHelper output,
    TestProxyFixture fixture,
    LiveServerFixture liveServerFixture)
    : AdmeRecordedTestsBase(output, fixture, liveServerFixture)
{
    private const string SchemaListTool = "adme_schema_list";
    private const string SchemaGetTool = "adme_schema_get";
    private const string WellEntityType = "master-data--Well";
    private const string WksSource = "wks";
    private const string OsduAuthority = "osdu";
    private const string WellKind = "osdu:wks:master-data--Well:1.0.0";
    private const string WellSchemaIdPrefix = "osdu:wks:master-data--Well:";
    private const string NonexistentKind = "osdu:wks:made-up--DoesNotExist:9.9.9";

    /// <summary>Verifies that the server exposes the schema tools.</summary>
    [Fact]
    public async Task Server_exposes_schema_tools_over_mcp()
    {
        var toolNames = await ListToolNamesAsync();

        Assert.Contains(SchemaListTool, toolNames);
        Assert.Contains(SchemaGetTool, toolNames);
    }

    /// <summary>Verifies that schema list returns descriptors for well entities.</summary>
    [Fact]
    public async Task SchemaList_returns_descriptors_for_well_entity()
    {
        var arguments = CreateArguments();
        arguments["entity-type"] = WellEntityType;
        arguments["source"] = WksSource;

        var result = await CallToolResultsAsync(SchemaListTool, arguments);
        var infos = result.GetProperty("schemaInfos");

        Assert.NotEmpty(infos.EnumerateArray());
        Assert.All(infos.EnumerateArray(), info =>
            Assert.StartsWith(
                WellSchemaIdPrefix,
                info.GetProperty("schemaIdentity").GetProperty("id").GetString()));
    }

    /// <summary>Verifies that latest-version filtering returns one schema version.</summary>
    [Fact]
    public async Task SchemaList_latestVersion_narrows_to_single_version()
    {
        var allArguments = CreateArguments();
        allArguments["authority"] = OsduAuthority;
        allArguments["entity-type"] = WellEntityType;
        allArguments["source"] = WksSource;
        var latestArguments = new Dictionary<string, object?>(allArguments)
        {
            ["latest-version"] = true,
        };

        var all = await CallToolResultsAsync(SchemaListTool, allArguments);
        var latest = await CallToolResultsAsync(SchemaListTool, latestArguments);

        var allCount = all.GetProperty("schemaInfos").GetArrayLength();
        var latestList = latest.GetProperty("schemaInfos");
        Assert.Equal(1, latestList.GetArrayLength());
        Assert.True(allCount >= latestList.GetArrayLength());
    }

    /// <summary>Verifies that schema list can return multiple source schemas.</summary>
    [Fact]
    public async Task SchemaList_fetches_multiple_schemas_from_source()
    {
        var arguments = CreateArguments();
        arguments["source"] = WksSource;
        arguments["limit"] = 5;

        var result = await CallToolResultsAsync(SchemaListTool, arguments);

        Assert.True(result.GetProperty("schemaInfos").GetArrayLength() > 1);
    }

    /// <summary>Verifies that version filters return exact schema versions.</summary>
    [Fact]
    public async Task SchemaList_version_filters_return_exact_match()
    {
        var arguments = CreateArguments();
        arguments["authority"] = OsduAuthority;
        arguments["source"] = WksSource;
        arguments["entity-type"] = WellEntityType;
        arguments["schema-version-major"] = 1;
        arguments["schema-version-minor"] = 0;
        arguments["limit"] = 3;

        var result = await CallToolResultsAsync(SchemaListTool, arguments);
        var infos = result.GetProperty("schemaInfos");

        Assert.NotEmpty(infos.EnumerateArray());
        Assert.All(infos.EnumerateArray(), info =>
        {
            var identity = info.GetProperty("schemaIdentity");
            Assert.Equal(1, identity.GetProperty("schemaVersionMajor").GetInt32());
            Assert.Equal(0, identity.GetProperty("schemaVersionMinor").GetInt32());
        });
    }

    /// <summary>Verifies that status filtering narrows schema results.</summary>
    [Fact]
    public async Task SchemaList_status_filter_narrows_results()
    {
        var allArguments = CreateArguments();
        allArguments["source"] = WksSource;
        allArguments["limit"] = 1000;
        var publishedArguments = new Dictionary<string, object?>(allArguments)
        {
            ["status"] = "PUBLISHED",
        };

        var all = await CallToolResultsAsync(SchemaListTool, allArguments);
        var published = await CallToolResultsAsync(SchemaListTool, publishedArguments);

        var allTotal = all.GetProperty("totalCount").GetInt32();
        var publishedTotal = published.GetProperty("totalCount").GetInt32();
        Assert.True(publishedTotal > 0);
        Assert.True(publishedTotal <= allTotal);
        Assert.All(
            published.GetProperty("schemaInfos").EnumerateArray(),
            info => Assert.Equal("PUBLISHED", info.GetProperty("status").GetString()));
    }

    /// <summary>Verifies that an offset advances schema pagination.</summary>
    [Fact]
    public async Task SchemaList_offset_advances_to_a_different_page()
    {
        var page1Arguments = CreateArguments();
        page1Arguments["source"] = WksSource;
        page1Arguments["offset"] = 0;
        page1Arguments["limit"] = 2;
        var page2Arguments = new Dictionary<string, object?>(page1Arguments)
        {
            ["offset"] = 2,
        };

        var page1 = await CallToolResultsAsync(SchemaListTool, page1Arguments);
        var page2 = await CallToolResultsAsync(SchemaListTool, page2Arguments);

        var page1Ids = SchemaIds(page1);
        var page2Ids = SchemaIds(page2);
        Assert.NotEmpty(page1Ids);
        Assert.NotEmpty(page2Ids);
        Assert.All(page2Ids, id => Assert.DoesNotContain(id, page1Ids));
    }

    /// <summary>Verifies that schema get returns data properties.</summary>
    [Fact]
    public async Task SchemaGet_returns_full_schema_with_data_properties()
    {
        var arguments = CreateArguments();
        arguments["kind"] = WellKind;

        var result = await CallToolResultsAsync(SchemaGetTool, arguments);

        Assert.True(
            result.TryGetProperty("properties", out var properties) &&
            properties.TryGetProperty("data", out _));
    }

    /// <summary>Verifies that an unknown schema kind returns an error.</summary>
    [Fact]
    public async Task SchemaGet_nonexistent_kind_returns_error()
    {
        var arguments = CreateArguments();
        arguments["kind"] = NonexistentKind;

        Assert.True(await CallToolReturnsErrorAsync(SchemaGetTool, arguments));
    }

    private static IReadOnlyCollection<string?> SchemaIds(JsonElement listResponse) =>
        listResponse.GetProperty("schemaInfos").EnumerateArray()
            .Select(info => info.GetProperty("schemaIdentity").GetProperty("id").GetString())
            .ToHashSet();
}
