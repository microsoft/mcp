// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Concurrent;
using System.Text.Json;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using Xunit;

namespace Azure.Mcp.Server.Tests.Infrastructure;

/// <summary>
/// Checks Monitor discovery and schemas without querying Azure.
/// Stateless requests are covered by <see cref="ServerModeCoverageTests"/>.
/// </summary>
public sealed class MonitorLogSearchServerModeTests
{
    private const string SearchTool = "monitor_workspace_log_search";
    private const string ConsolidatedTool = "get_azure_resource_and_app_health_status";
    private const string ConsolidatedSearchCommand =
        "get_azure_resource_and_app_health_status_monitor_workspace_log_search";
    private const string LearnIntent = "Discover Monitor log search commands.";

    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(120);

    private static string AzmcpPath =>
        Path.Combine(AppContext.BaseDirectory, OperatingSystem.IsWindows() ? "azmcp.exe" : "azmcp");

    [Theory]
    [InlineData("server start --mode all --namespace monitor --structured-output-mode duplicated")]
    [InlineData("server start --tool " + SearchTool + " --structured-output-mode duplicated")]
    public async Task DirectModes_ExposeSearchToolWithOutputSchema(string arguments)
    {
        await using var server = await ServerSession.StartAsync(arguments);

        var tools = await server.ListToolsAsync();
        var search = Assert.Single(tools, tool => tool.Name == SearchTool);
        var schema = RequireOutputSchema(search);

        AssertTypeIncludes(schema.GetProperty("type"), "object");
        var properties = schema.GetProperty("properties");
        Assert.True(properties.TryGetProperty("rows", out var rows));
        AssertTypeIncludes(rows.GetProperty("type"), "array");
        AssertTypeIncludes(rows.GetProperty("items").GetProperty("type"), "array");

        var errorType = properties.GetProperty("error").GetProperty("type");
        AssertTypeIncludes(errorType, "object");
        AssertTypeIncludes(errorType, "null");
    }

    [Theory]
    [InlineData("server start --namespace monitor --structured-output-mode duplicated")]
    [InlineData("server start --mode namespace --namespace monitor --structured-output-mode duplicated")]
    public async Task NamespaceModes_WithMonitorFilter_DiscoverSearchCommand(string arguments)
    {
        await using var server = await ServerSession.StartAsync(arguments);

        var tools = await server.ListToolsAsync();
        var monitor = Assert.Single(tools, tool => tool.Name == "monitor");
        AssertAggregateSchema(RequireOutputSchema(monitor), includesTool: false);

        var structuredContent = await server.LearnAsync("monitor");
        AssertContainsCommand(structuredContent, SearchTool);
    }

    [Fact]
    public async Task SingleMode_WithMonitorFilter_DiscoversSearchCommand()
    {
        await using var server = await ServerSession.StartAsync(
            "server start --mode single --namespace monitor --structured-output-mode duplicated");

        var azure = Assert.Single(await server.ListToolsAsync());
        Assert.Equal("azure", azure.Name);
        AssertAggregateSchema(RequireOutputSchema(azure), includesTool: true);

        var structuredContent = await server.LearnAsync("azure", tool: "monitor");
        AssertContainsCommand(structuredContent, SearchTool);
    }

    [Fact]
    public async Task ConsolidatedMode_WithMonitorFilter_DiscoversMappedSearchCommand()
    {
        await using var server = await ServerSession.StartAsync(
            "server start --mode consolidated --namespace monitor --structured-output-mode duplicated");

        var consolidated = Assert.Single(await server.ListToolsAsync(), tool => tool.Name == ConsolidatedTool);
        Assert.Contains(
            "search Basic and Auxiliary tables in a Log Analytics workspace",
            consolidated.Description,
            StringComparison.OrdinalIgnoreCase);
        AssertAggregateSchema(RequireOutputSchema(consolidated), includesTool: false);

        var structuredContent = await server.LearnAsync(ConsolidatedTool);
        AssertContainsCommand(structuredContent, ConsolidatedSearchCommand);
    }

    private static JsonElement RequireOutputSchema(Tool tool)
    {
        Assert.True(tool.OutputSchema.HasValue, $"Tool '{tool.Name}' did not advertise an output schema.");
        return tool.OutputSchema!.Value;
    }

    private static void AssertContainsCommand(JsonElement structuredContent, string expectedCommand)
    {
        Assert.Equal("tool-list", structuredContent.GetProperty("kind").GetString());

        var commands = structuredContent.GetProperty("tools")
            .EnumerateArray()
            .Select(tool => tool.GetProperty("command").GetString())
            .ToList();

        Assert.Contains(expectedCommand, commands);
    }

    private static void AssertTypeIncludes(JsonElement type, string expected)
    {
        if (type.ValueKind == JsonValueKind.String)
        {
            Assert.Equal(expected, type.GetString());
            return;
        }

        Assert.Contains(expected, type.EnumerateArray().Select(item => item.GetString()));
    }

    private static void AssertAggregateSchema(JsonElement schema, bool includesTool)
    {
        var variants = schema.GetProperty("oneOf").EnumerateArray().ToList();
        var toolResult = Assert.Single(
            variants,
            variant =>
                variant.GetProperty("properties")
                    .GetProperty("kind")
                    .GetProperty("const")
                    .GetString() == "tool-result");
        var required = toolResult.GetProperty("required")
            .EnumerateArray()
            .Select(item => item.GetString())
            .ToList();

        Assert.Contains("command", required);
        Assert.Contains("result", required);
        Assert.Equal(includesTool, required.Contains("tool"));
    }

    private sealed class ServerSession(McpClient client, string arguments, ConcurrentQueue<string> standardError)
        : IAsyncDisposable
    {
        public static async Task<ServerSession> StartAsync(string arguments)
        {
            Assert.True(File.Exists(AzmcpPath), $"Executable not found at {AzmcpPath}. Build Azure.Mcp.Server first.");

            var standardError = new ConcurrentQueue<string>();
            var transport = new StdioClientTransport(new StdioClientTransportOptions
            {
                Name = "monitor-log-search-mode-test",
                Command = AzmcpPath,
                Arguments = arguments.Split(' ', StringSplitOptions.RemoveEmptyEntries),
                StandardErrorLines = standardError.Enqueue
            });

            var client = await McpClient.CreateAsync(
                transport,
                new McpClientOptions { InitializationTimeout = Timeout },
                cancellationToken: TestContext.Current.CancellationToken);

            return new ServerSession(client, arguments, standardError);
        }

        public async Task<IReadOnlyList<Tool>> ListToolsAsync()
        {
            using var cancellation = CreateRequestCancellation();

            var tools = new List<Tool>();
            string? cursor = null;
            do
            {
                var result = await client.ListToolsAsync(
                    new ListToolsRequestParams { Cursor = cursor },
                    cancellation.Token);
                tools.AddRange(result.Tools);
                cursor = result.NextCursor;
            }
            while (cursor is not null);

            return tools;
        }

        public async Task<JsonElement> LearnAsync(string toolName, string? tool = null)
        {
            var callArguments = new Dictionary<string, object?>
            {
                ["intent"] = LearnIntent,
                ["learn"] = true
            };

            if (tool is not null)
            {
                callArguments["tool"] = tool;
            }

            using var cancellation = CreateRequestCancellation();
            var result = await client.CallToolAsync(toolName, callArguments, cancellationToken: cancellation.Token);

            Assert.False(
                result.IsError == true,
                $"'{toolName}' learn call failed for 'azmcp {arguments}'. StdErr: {Describe(standardError)}");
            Assert.True(
                result.StructuredContent.HasValue,
                $"'{toolName}' learn call returned no structured content for 'azmcp {arguments}'.");

            return result.StructuredContent!.Value;
        }

        public ValueTask DisposeAsync() => client.DisposeAsync();

        private static CancellationTokenSource CreateRequestCancellation()
        {
            var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
            cancellation.CancelAfter(Timeout);
            return cancellation;
        }

        private static string Describe(ConcurrentQueue<string> standardError) =>
            standardError.IsEmpty ? "<empty>" : string.Join(Environment.NewLine, standardError);
    }
}
