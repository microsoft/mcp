// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace McpOutputSizeMeasurer;

/// <summary>
/// Measures MCP responses from a client perspective for a server's exposed tool surfaces.
/// Starts the given azmcp-compatible executable over stdio, walks tool discovery
/// (tools/list, paginated), calls every tool's learn-mode response, and re-queries every
/// inner command a tool's learn response advertises. All requests/responses are exchanged
/// as raw JSON-RPC text so that UTF-8 byte counts reflect exactly what crosses the wire.
/// </summary>
public sealed class McpOutputSizeMeasurer
{
    private readonly Action<string>? _logger;

    public McpOutputSizeMeasurer(Action<string>? logger = null)
    {
        _logger = logger;
    }

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

        using var process = Process.Start(new ProcessStartInfo
        {
            FileName = executablePath,
            Arguments = $"server start --mode {mode}",
            UseShellExecute = false,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = false,
            CreateNoWindow = true
        }) ?? throw new InvalidOperationException($"Failed to start process for {executablePath}.");

        try
        {
            await Task.Delay(500, cancellationToken);

            var initializeResponse = await SendRequestAsync(
                process,
                """
                {"jsonrpc":"2.0","id":1,"method":"initialize","params":{"protocolVersion":"2025-11-25","capabilities":{},"clientInfo":{"name":"mcp-output-size-measurer","version":"1.0"}}}
                """,
                cancellationToken);

            await SendNotificationAsync(
                process,
                """{"jsonrpc":"2.0","method":"notifications/initialized"}""",
                cancellationToken);

            var discoveryResponses = new List<object>();
            var discoveryTexts = new List<string>();
            var tools = new List<string>();
            var discoveryTotalUtf8Bytes = 0;
            string? cursor = null;
            var requestId = 2;

            do
            {
                var request = cursor is null
                    ? JsonSerializer.Serialize(new
                    {
                        jsonrpc = "2.0",
                        id = requestId,
                        method = "tools/list",
                        @params = new { }
                    })
                    : JsonSerializer.Serialize(new
                    {
                        jsonrpc = "2.0",
                        id = requestId,
                        method = "tools/list",
                        @params = new { cursor }
                    });
                var response = await SendRequestAsync(process, request, cancellationToken);
                discoveryTexts.Add(response);
                using var document = JsonDocument.Parse(response);
                var result = document.RootElement.GetProperty("result");
                foreach (var tool in result.GetProperty("tools").EnumerateArray())
                {
                    tools.Add(tool.GetProperty("name").GetString()!);
                }

                cursor = result.TryGetProperty("nextCursor", out var nextCursor)
                    ? nextCursor.GetString()
                    : null;
                discoveryTotalUtf8Bytes += GetUtf8ByteCount(response);
                discoveryResponses.Add(new
                {
                    messageNumber = discoveryResponses.Count + 1,
                    utf8Bytes = GetUtf8ByteCount(response),
                    characterCount = response.Length,
                    toolCount = result.GetProperty("tools").GetArrayLength()
                });
                requestId++;
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
            var learnResponseTextByTool = new Dictionary<string, string>(tools.Count, StringComparer.Ordinal);
            var learnTotalUtf8Bytes = 0;
            foreach (var tool in tools)
            {
                var request = JsonSerializer.Serialize(new
                {
                    jsonrpc = "2.0",
                    id = requestId,
                    method = "tools/call",
                    @params = new
                    {
                        name = tool,
                        arguments = new
                        {
                            intent = "Measure available commands",
                            learn = true
                        }
                    }
                });
                var response = await SendRequestAsync(process, request, cancellationToken);
                using var document = JsonDocument.Parse(response);
                if (!document.RootElement.TryGetProperty("result", out _))
                {
                    throw new InvalidOperationException($"The learn response for '{tool}' did not contain a result.");
                }

                learnTotalUtf8Bytes += GetUtf8ByteCount(response);
                learnResponseTextByTool[tool] = response;

                var learnResponseFile = Path.Combine(
                    learnDirectory,
                    $"{SanitizeFileNameSegment(tool)}.json");
                await File.WriteAllTextAsync(learnResponseFile, response, Encoding.UTF8, cancellationToken);

                learnResponses.Add(new
                {
                    tool,
                    utf8Bytes = GetUtf8ByteCount(response),
                    characterCount = response.Length,
                    learnResponseFile
                });
                requestId++;
            }

            // Verify that per-command learn requests also return details, mirroring the
            // top-level tool discovery. Each inner command advertised by a tool's learn
            // response is re-requested with "command" set and "learn" still true.
            var commandLearnResponses = new List<object>();
            var commandLearnTotalUtf8Bytes = 0;
            foreach (var tool in tools)
            {
                foreach (var command in GetInnerCommandNames(learnResponseTextByTool[tool]))
                {
                    var request = JsonSerializer.Serialize(new
                    {
                        jsonrpc = "2.0",
                        id = requestId,
                        method = "tools/call",
                        @params = new
                        {
                            name = tool,
                            arguments = new
                            {
                                intent = "Measure command details",
                                command,
                                learn = true,
                                parameters = new { }
                            }
                        }
                    });
                    var response = await SendRequestAsync(process, request, cancellationToken);
                    using var document = JsonDocument.Parse(response);
                    if (!document.RootElement.TryGetProperty("result", out var commandResult))
                    {
                        throw new InvalidOperationException($"The learn response for '{tool}.{command}' did not contain a result.");
                    }
                    if (!commandResult.TryGetProperty("content", out var commandContent) ||
                        commandContent.GetArrayLength() == 0)
                    {
                        throw new InvalidOperationException($"The learn response for '{tool}.{command}' did not contain any content.");
                    }

                    commandLearnTotalUtf8Bytes += GetUtf8ByteCount(response);
                    commandLearnResponses.Add(new
                    {
                        tool,
                        command,
                        utf8Bytes = GetUtf8ByteCount(response),
                        characterCount = response.Length
                    });
                    requestId++;
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
                initialGreetingResponse = MeasureMessage(initializeResponse),
                discoveryTextFile = discoveryTextPath,
                discoveryResponses,
                discoveryTotalUtf8Bytes,
                learnResponses,
                learnTotalUtf8Bytes,
                commandLearnCount = commandLearnResponses.Count,
                commandLearnTotalUtf8Bytes,
                commandLearnResponses,
                totalUtf8Bytes = GetUtf8ByteCount(initializeResponse) +
                    discoveryTotalUtf8Bytes +
                    learnTotalUtf8Bytes
            };
        }
        finally
        {
            if (!process.HasExited)
            {
                process.Kill();
            }
        }
    }

    private async Task<string> SendRequestAsync(Process process, string request, CancellationToken cancellationToken)
    {
        _logger?.Invoke($"--> {request}");
        await process.StandardInput.WriteLineAsync(request);
        await process.StandardInput.FlushAsync(cancellationToken);

        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(30));
        var response = await process.StandardOutput.ReadLineAsync(timeout.Token)
            ?? throw new InvalidOperationException("The server closed its output stream before responding.");
        _logger?.Invoke($"<-- {response}");
        return response;
    }

    private static async Task SendNotificationAsync(Process process, string notification, CancellationToken cancellationToken)
    {
        await process.StandardInput.WriteLineAsync(notification);
        await process.StandardInput.FlushAsync(cancellationToken);
    }

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
    /// Parses the inner command names from a tool's learn response. The learn text is a short
    /// preamble followed by a JSON array of command descriptors.
    /// </summary>
    internal static List<string> GetInnerCommandNames(string learnResponse)
    {
        var commands = new List<string>();

        using var document = JsonDocument.Parse(learnResponse);
        if (!document.RootElement.TryGetProperty("result", out var result) ||
            !result.TryGetProperty("content", out var content))
        {
            return commands;
        }

        foreach (var block in content.EnumerateArray())
        {
            if (!block.TryGetProperty("text", out var textElement))
            {
                continue;
            }

            var text = textElement.GetString();
            var start = text?.IndexOf('[') ?? -1;
            if (text is null || start < 0)
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
                if (commandDocument.RootElement.ValueKind != JsonValueKind.Array)
                {
                    continue;
                }

                foreach (var command in commandDocument.RootElement.EnumerateArray())
                {
                    if (command.TryGetProperty("command", out var name) &&
                        name.GetString() is { Length: > 0 } commandName)
                    {
                        commands.Add(commandName);
                    }
                }
            }
        }

        return commands;
    }
}
