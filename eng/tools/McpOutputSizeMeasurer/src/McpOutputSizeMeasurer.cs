// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text;
using System.Text.Json;
using ModelContextProtocol;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;

namespace McpOutputSizeMeasurer;

/// <summary>
/// Measures MCP responses from a client perspective for a server's exposed tool surfaces.
/// Starts the given azmcp-compatible executable using the MCP SDK's stdio transport, walks tool discovery
/// (tools/list, paginated), calls every tool's learn-mode response, and re-queries every
/// inner command a tool's learn response advertises. Learn-response sizes measure the decoded
/// command-array JSON used by the server's discovery threshold, not its JSON-RPC wire encoding.
/// </summary>
public sealed class McpOutputSizeMeasurer(
    Action<string>? logger = null,
    TimeSpan? requestTimeout = null)
{
    private static readonly TimeSpan DefaultRequestTimeout = TimeSpan.FromMinutes(2);
    private readonly Action<string>? _logger = logger;
    private readonly TimeSpan _requestTimeout = requestTimeout ?? DefaultRequestTimeout;

    /// <summary>
    /// Runs the full measurement workflow for every mode in <paramref name="modes"/> and
    /// returns a report object with the same shape previously produced by
    /// Report shape: <c>{ transport, generatedAtUtc, reportPath, modes }</c>.
    /// </summary>
    public async Task<object> MeasureAsync(
        string executablePath,
        IReadOnlyList<string> modes,
        string reportDirectory,
        string reportPath,
        CancellationToken cancellationToken = default)
    {
        var measurements = new List<object>();
        foreach (var mode in modes)
        {
            measurements.Add(await MeasureModeAsync(executablePath, mode, reportDirectory, cancellationToken));
        }

        return new
        {
            transport = "stdio",
            protocolClient = "ModelContextProtocol SDK",
            learnSizeBasis = "decoded command-array JSON; responses without command JSON are reported separately",
            generatedAtUtc = DateTimeOffset.UtcNow,
            reportPath,
            modes = measurements
        };
    }

    public async Task<object> MeasureModeAsync(
        string executablePath,
        string mode,
        string reportDirectory,
        CancellationToken cancellationToken = default)
    {
        if (!File.Exists(executablePath))
        {
            throw new FileNotFoundException(
                $"Executable not found at {executablePath}. Build the server project first.",
                executablePath);
        }

        var transport = new StdioClientTransport(new StdioClientTransportOptions
        {
            Name = $"MCP output size measurer ({mode})",
            Command = executablePath,
            Arguments = ["server", "start", "--mode", mode],
            StandardErrorLines = line => _logger?.Invoke($"[MCP Server] {line}")
        });
        var client = await WaitForResponseAsync(
            token => McpClient.CreateAsync(
                transport,
                new McpClientOptions { InitializationTimeout = _requestTimeout },
                cancellationToken: token),
            _requestTimeout,
            "MCP client connection",
            cancellationToken);

        await using (client)
        {
            var serverMetadataPayload = JsonSerializer.Serialize(
                new
                {
                    client.ServerInfo,
                    client.ServerCapabilities,
                    client.ServerInstructions
                },
                McpJsonUtilities.DefaultOptions);

            var discoveryResponses = new List<object>();
            var discoveryTexts = new List<string>();
            var tools = new List<string>();
            var discoveryTotalUtf8Bytes = 0;
            string? cursor = null;

            do
            {
                var result = await WaitForResponseAsync<ListToolsResult>(
                    token => client.ListToolsAsync(
                        new ListToolsRequestParams { Cursor = cursor },
                        token).AsTask(),
                    _requestTimeout,
                    "tools/list",
                    cancellationToken);
                var response = SerializeProtocolPayload(result);
                discoveryTexts.Add(response);
                foreach (var tool in result.Tools)
                {
                    tools.Add(tool.Name);
                }

                cursor = result.NextCursor;
                discoveryTotalUtf8Bytes += GetUtf8ByteCount(response);
                discoveryResponses.Add(new
                {
                    messageNumber = discoveryResponses.Count + 1,
                    utf8Bytes = GetUtf8ByteCount(response),
                    characterCount = response.Length,
                    toolCount = result.Tools.Count
                });
            }
            while (cursor is not null);

            if (tools.Count == 0)
            {
                throw new InvalidOperationException($"No tools were discovered for mode '{mode}'.");
            }

            // Save every tool's learn response in a per-mode subdirectory so any payload can be
            // inspected without re-running the server. The directory is cleared first so stale
            // files from a previous run (e.g. a tool that no longer produces the largest
            // response) don't linger alongside the current run's files. Filtering down to the
            // largest/most interesting responses is left to the summarization script, which
            // operates on this full, unfiltered set.
            var learnDirectory = Path.Combine(reportDirectory, mode);
            if (Directory.Exists(learnDirectory))
            {
                Directory.Delete(learnDirectory, recursive: true);
            }
            Directory.CreateDirectory(learnDirectory);

            var learnResponses = new List<object>(tools.Count);
            var learnResponseByTool = new Dictionary<string, CallToolResult>(tools.Count, StringComparer.Ordinal);
            var learnTotalUtf8Bytes = 0;
            var learnResponsePayloadTotalUtf8Bytes = 0;
            var learnResponsesWithoutCommandJson = 0;
            foreach (var tool in tools)
            {
                var result = await WaitForResponseAsync<CallToolResult>(
                    token => client.CallToolAsync(
                        tool,
                        new Dictionary<string, object?>
                        {
                            ["intent"] = "Measure available commands",
                            ["learn"] = true
                        },
                        cancellationToken: token).AsTask(),
                    _requestTimeout,
                    $"tools/call ({tool})",
                    cancellationToken);
                if (result.Content.Count == 0)
                {
                    throw new InvalidOperationException($"The learn response for '{tool}' did not contain any content.");
                }

                var response = SerializeProtocolPayload(result);
                var learnPayload = MeasureLearnPayload(result);
                var responseUtf8Bytes = GetUtf8ByteCount(response);
                learnTotalUtf8Bytes += learnPayload.Utf8Bytes ?? 0;
                learnResponsePayloadTotalUtf8Bytes += responseUtf8Bytes;
                if (learnPayload.Utf8Bytes is null)
                {
                    learnResponsesWithoutCommandJson++;
                }
                learnResponseByTool[tool] = result;

                var learnResponseFile = Path.Combine(
                    learnDirectory,
                    $"{SanitizeFileNameSegment(tool)}.json");
                await File.WriteAllTextAsync(learnResponseFile, response, Encoding.UTF8, cancellationToken);

                learnResponses.Add(new
                {
                    tool,
                    utf8Bytes = learnPayload.Utf8Bytes,
                    characterCount = learnPayload.CharacterCount,
                    sizeBasis = learnPayload.SizeBasis,
                    decodedContentUtf8Bytes = learnPayload.DecodedContentUtf8Bytes,
                    responsePayloadUtf8Bytes = responseUtf8Bytes,
                    learnResponseFile
                });
            }

            // Verify that per-command learn requests also return details, mirroring the
            // top-level tool discovery. Each inner command advertised by a tool's learn
            // response is re-requested with "command" set and "learn" still true.
            var commandLearnResponses = new List<object>();
            var commandLearnTotalUtf8Bytes = 0;
            foreach (var tool in tools)
            {
                foreach (var command in GetInnerCommandNames(learnResponseByTool[tool]))
                {
                    var result = await WaitForResponseAsync<CallToolResult>(
                        token => client.CallToolAsync(
                            tool,
                            new Dictionary<string, object?>
                            {
                                ["intent"] = "Measure command details",
                                ["command"] = command,
                                ["learn"] = true,
                                ["parameters"] = new Dictionary<string, object?>()
                            },
                            cancellationToken: token).AsTask(),
                        _requestTimeout,
                        $"tools/call ({tool}.{command})",
                        cancellationToken);
                    if (result.Content.Count == 0)
                    {
                        throw new InvalidOperationException(
                            $"The learn response for '{tool}.{command}' did not contain any content.");
                    }

                    var response = SerializeProtocolPayload(result);
                    commandLearnTotalUtf8Bytes += GetUtf8ByteCount(response);
                    commandLearnResponses.Add(new
                    {
                        tool,
                        command,
                        utf8Bytes = GetUtf8ByteCount(response),
                        characterCount = response.Length
                    });
                }
            }

            var discoveryTextPath = Path.Combine(
                reportDirectory,
                $"mcp-output-size-{mode}-discovery.jsonl");
            await File.WriteAllTextAsync(
                discoveryTextPath,
                string.Join(Environment.NewLine, discoveryTexts) + Environment.NewLine,
                Encoding.UTF8,
                cancellationToken);

            return new
            {
                mode,
                toolCount = tools.Count,
                initialServerMetadata = MeasureMessage(serverMetadataPayload),
                discoveryTextFile = discoveryTextPath,
                discoveryResponses,
                discoveryTotalUtf8Bytes,
                learnResponses,
                learnTotalUtf8Bytes,
                learnResponsePayloadTotalUtf8Bytes,
                learnResponsesWithoutCommandJson,
                commandLearnCount = commandLearnResponses.Count,
                commandLearnTotalUtf8Bytes,
                commandLearnResponses,
                totalMeasuredPayloadUtf8Bytes = GetUtf8ByteCount(serverMetadataPayload) +
                    discoveryTotalUtf8Bytes +
                    learnResponsePayloadTotalUtf8Bytes +
                    commandLearnTotalUtf8Bytes
            };
        }
    }

    internal static async Task<T> WaitForResponseAsync<T>(
        Func<CancellationToken, Task<T>> request,
        TimeSpan timeout,
        string requestDescription,
        CancellationToken cancellationToken)
    {
        using var timeoutCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCancellation.CancelAfter(timeout);

        try
        {
            return await request(timeoutCancellation.Token).WaitAsync(timeoutCancellation.Token);
        }
        catch (OperationCanceledException ex)
            when (!cancellationToken.IsCancellationRequested && timeoutCancellation.IsCancellationRequested)
        {
            throw new TimeoutException(
                $"The MCP server did not respond to '{requestDescription}' within {timeout.TotalSeconds:N0} seconds.",
                ex);
        }
        catch (TimeoutException ex)
        {
            throw new TimeoutException(
                $"The MCP server did not respond to '{requestDescription}' within {timeout.TotalSeconds:N0} seconds.",
                ex);
        }
    }

    private static string SerializeProtocolPayload<T>(T payload)
        => JsonSerializer.Serialize(payload, McpJsonUtilities.DefaultOptions);

    private static object MeasureMessage(string message) => new
    {
        utf8Bytes = GetUtf8ByteCount(message),
        characterCount = message.Length
    };

    private static int GetUtf8ByteCount(string message)
        => Encoding.UTF8.GetByteCount(message);

    private static string SanitizeFileNameSegment(string value)
    {
        var sanitized = value;
        foreach (var invalidChar in Path.GetInvalidFileNameChars())
        {
            sanitized = sanitized.Replace(invalidChar, '-');
        }

        return sanitized;
    }

    /// <summary>
    /// Extracts the decoded command array from a tool's learn response. This is the same internal
    /// JSON payload used by the server when deciding whether to switch discovery strategies.
    /// </summary>
    internal static string? GetInnerCommandsJson(CallToolResult learnResponse)
    {
        foreach (var block in learnResponse.Content)
        {
            if (block is not TextContentBlock textBlock)
            {
                continue;
            }

            var text = textBlock.Text;
            var start = text.IndexOf('[');
            if (start < 0)
            {
                continue;
            }

            JsonDocument commandDocument;
            try
            {
                commandDocument = JsonDocument.Parse(text[start..]);
            }
            catch (JsonException)
            {
                continue;
            }

            using (commandDocument)
            {
                if (commandDocument.RootElement.ValueKind == JsonValueKind.Array)
                {
                    return text[start..];
                }
            }
        }

        return null;
    }

    internal static string? GetDecodedContentText(CallToolResult learnResponse)
    {
        var textBlocks = new List<string>();
        foreach (var block in learnResponse.Content)
        {
            if (block is TextContentBlock textBlock)
            {
                textBlocks.Add(textBlock.Text);
            }
        }

        return textBlocks.Count == 0
            ? null
            : string.Join(Environment.NewLine + Environment.NewLine, textBlocks);
    }

    internal static (
        int? Utf8Bytes,
        int? CharacterCount,
        string SizeBasis,
        int? DecodedContentUtf8Bytes) MeasureLearnPayload(CallToolResult learnResponse)
    {
        var innerCommandsJson = GetInnerCommandsJson(learnResponse);
        if (innerCommandsJson is not null)
        {
            return (
                GetUtf8ByteCount(innerCommandsJson),
                innerCommandsJson.Length,
                "decodedCommandJson",
                null);
        }

        var decodedContentText = GetDecodedContentText(learnResponse)
            ?? throw new InvalidOperationException(
                "The learn response did not contain decoded text content.");
        return (
            null,
            null,
            "decodedContentTextOnly",
            GetUtf8ByteCount(decodedContentText));
    }

    /// <summary>
    /// Parses the inner command names from a tool's decoded command array.
    /// </summary>
    internal static List<string> GetInnerCommandNames(CallToolResult learnResponse)
    {
        var commands = new List<string>();
        var innerCommandsJson = GetInnerCommandsJson(learnResponse);
        if (innerCommandsJson is null)
        {
            return commands;
        }

        using var commandDocument = JsonDocument.Parse(innerCommandsJson);
        foreach (var command in commandDocument.RootElement.EnumerateArray())
        {
            if (command.TryGetProperty("command", out var name) &&
                name.GetString() is { Length: > 0 } commandName)
            {
                commands.Add(commandName);
            }
        }

        return commands;
    }
}
