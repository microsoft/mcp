// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Microsoft.Mcp.Tests.Client;
using Microsoft.Mcp.Tests.Client.Helpers;
using Microsoft.Mcp.Tests.Generated.Models;
using Xunit;

namespace Azure.Mcp.Tools.Adme.Tests.Recorded;

/// <summary>Provides shared infrastructure for ADME recorded tests.</summary>
public abstract class AdmeRecordedTestsBase(
    ITestOutputHelper output,
    TestProxyFixture fixture,
    LiveServerFixture liveServerFixture)
    : RecordedCommandTestsBase(output, fixture, liveServerFixture)
{
    private const string PlaybackEndpoint = "https://recording.energy.azure.com";
    private const string PlaybackDataPartition = "recording-partition";

    /// <inheritdoc />
    public override List<HeaderRegexSanitizer> HeaderRegexSanitizers =>
    [
        .. base.HeaderRegexSanitizers,
        new(new("data-partition-id")
        {
            Value = PlaybackDataPartition,
        }),
    ];

    /// <inheritdoc />
    public override List<UriRegexSanitizer> UriRegexSanitizers =>
    [
        .. base.UriRegexSanitizers,
        new(new()
        {
            Regex = "https://[^/]+",
            Value = PlaybackEndpoint,
        }),
    ];

    protected async Task<HashSet<string>> ListToolNamesAsync() =>
        (await Client.ListToolsAsync()).Select(tool => tool.Name).ToHashSet();

    protected async Task<JsonElement> CallToolResultsAsync(
        string tool,
        Dictionary<string, object?> arguments)
    {
        var result = await CallToolAsync(tool, arguments);
        Assert.NotNull(result);
        return result.Value;
    }

    protected async Task<bool> CallToolReturnsErrorAsync(
        string tool,
        Dictionary<string, object?> arguments) =>
        (await Client.CallToolAsync(tool, arguments)).IsError == true;

    protected Dictionary<string, object?> CreateArguments()
    {
        var endpoint = Settings.EnvironmentVariables.GetValueOrDefault("ADME_MCP_SERVER_URL")
            ?? PlaybackEndpoint;
        var dataPartition = Settings.EnvironmentVariables.GetValueOrDefault("ADME_MCP_SERVER_DATA_PARTITION")
            ?? PlaybackDataPartition;
        return new()
        {
            ["endpoint"] = endpoint,
            ["data-partition"] = dataPartition,
        };
    }
}
