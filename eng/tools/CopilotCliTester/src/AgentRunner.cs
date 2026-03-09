// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Data;
using System.Text;
using System.Text.RegularExpressions;
using CopilotCliTester.Models;
using GitHub.Copilot.SDK;

namespace CopilotCliTester;

/// <summary>
/// Agent runner for MCP E2E tests using Copilot SDK.
/// Creates CopilotClient + session with MCP Azure server, runs prompts,
/// collects events, and supports early termination when expected tool is invoked.
/// </summary>
internal sealed partial class AgentRunner : IAsyncDisposable
{
    private static readonly string DefaultReportDir = Path.Combine(AppContext.BaseDirectory, "reports");
    private static readonly string TimeStamp = DateTimeOffset.UtcNow.ToString("yyyyMMdd-HHmmss");

    [GeneratedRegex(@"eyJ[A-Za-z0-9_-]{10,}\.[A-Za-z0-9_-]{10,}\.[A-Za-z0-9_-]{10,}")]
    private static partial Regex JwtPattern();

    [GeneratedRegex(@"Bearer\s+[A-Za-z0-9_\-.~+/]{20,}", RegexOptions.IgnoreCase)]
    private static partial Regex BearerPattern();

    [GeneratedRegex(@"(?:password|secret|token|api[_-]?key)\s*[:=]\s*[""']?[^\s""',]{6,}", RegexOptions.IgnoreCase)]
    private static partial Regex SecretKeyValuePattern();

    private readonly CopilotClient _client;
    public AgentRunner(CopilotClient client)
    {
        _client = client;
    }

    public async ValueTask DisposeAsync()
    {
        await _client.DisposeAsync();
    }

    /// <summary>
    /// Creates a shared CopilotClient that should be reused across all test runs.
    /// One CLI process instead of one-per-prompt avoids file handle exhaustion (EMFILE).
    /// </summary>
    public static CopilotClient CreateSharedClient(bool debug = false)
    {
        var workspace = CreateTempWorkspace("mcp-test-");
        var cliArgs = new List<string> { "--yolo", "--allow-all-tools", "--allow-all-paths" };
        if (debug)
        {
            cliArgs.Add("--log-dir");
            cliArgs.Add(Path.Combine(DefaultReportDir, "copilot-logs"));
        }

        return new CopilotClient(new CopilotClientOptions
        {
            CliArgs = [.. cliArgs],
            Cwd = workspace,
        });
    }

    public async Task<AgentMetadata> RunAsync(AgentRunConfig config, CancellationToken cancellationToken = default)
    {
        var workspace = CreateTempWorkspace("mcp-test-");
        var isComplete = false;
        AgentMetadata? metadata = null;

        // Snapshot azmcp processes before session creation so we can kill leaked ones after
        // var preSessionPids = GetAzmcpProcessIds();

        try
        {
            var debug = config.Debug ?? !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DEBUG"));

            SystemMessageConfig? systemMessage = null;
            if (config.SystemPrompt is not null)
            {
                systemMessage = new SystemMessageConfig
                {
                    Mode = config.SystemPrompt.Mode == SystemPromptMode.Append 
                        ? SystemMessageMode.Append 
                        : SystemMessageMode.Replace,
                    Content = config.SystemPrompt.Content
                };
            }

            var session = await _client.CreateSessionAsync(new SessionConfig
            {
                Model = config.Model ?? "claude-sonnet-4.5",
                Streaming = true,
                OnPermissionRequest = PermissionHandler.ApproveAll,
                McpServers = new Dictionary<string, object>
                {
                    ["azure"] = new McpLocalServerConfig
                    {
                        Type = "stdio",
                        Command = @"C:\Users\anannyapatra\mcp-main-noChange\mcp\servers\Azure.Mcp.Server\src\bin\Debug\net10.0\azmcp.exe",
                        Args = ["server", "start", "--mode", "all"],
                        Tools = ["*"]
                    },
                },
                // McpServers = BuildMcpServersConfig(config.McpServerUrl),
                SystemMessage = systemMessage
            }, cancellationToken);

            try
            {
                metadata = new AgentMetadata();

                var idleTcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

                session.On(ev =>
                {
                    if (isComplete) return;

                    if (TryMapEvent(ev, out var mapped))
                    {
                        if (debug)
                            Console.WriteLine($"--- Session event: {mapped.Type}");

                        if (mapped.Type == "session.idle")
                        {
                            isComplete = true;
                            idleTcs.TrySetResult();
                            return;
                        }

                        metadata.Events.Add(mapped);

                        if (config.ShouldEarlyTerminate is not null && config.ShouldEarlyTerminate(metadata))
                        {
                            isComplete = true;
                            idleTcs.TrySetResult();
                            return;
                        }
                    }
                    else if (debug)
                    {
                        Console.WriteLine($"--- Unmapped event: {ev.GetType().Name}");
                    }
                });

                await session.SendAsync(new MessageOptions { Prompt = config.Prompt }, cancellationToken);

                // Wait for idle, caller controls timeout via cancellationToken
                try
                {
                    await idleTcs.Task.WaitAsync(cancellationToken);
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    isComplete = true;
                }

                WriteMarkdownReport(config, metadata);

                return metadata;
            }
            finally
            {
                try
                {
                    await session.AbortAsync(CancellationToken.None);
                }
                catch
                {
                    // Best-effort; continue to disposal
                }

                // Wait for process to actually terminate after abort signal as AbortAsync may only signal termination without waiting for exit.
                await Task.Delay(500, CancellationToken.None);

                // Dispose the session after aborting
                try
                {
                    await session.DisposeAsync();
                }
                catch
                {
                    // Best-effort; ignore disposal errors
                }

                // Kill any azmcp.exe processes that were spawned during this session
                // and survived the abort/dispose. This is a safety net because the SDK
                // doesn't always reliably terminate child stdio processes.
                // KillLeakedAzmcpProcesses(preSessionPids);
            }
        }
        catch (Exception ex)
        {
            isComplete = true;
            Console.Error.WriteLine($"Agent runner error: {ex}");
            if (metadata is not null)
            {
                try
                {
                    WriteMarkdownReport(config, metadata);
                }
                catch (Exception reportEx)
                {
                    Console.Error.WriteLine($"Warning: Failed to write report: {reportEx.Message}");
                }
            }
            
            throw;
        }
        finally
        {
            // Clean up workspace, ignore failures, OS will clean `temp` eventually
            for(int i=0;i <3; i++)
            {
                try
                {
                    if (Directory.Exists(workspace))
                    {
                        await Task.Delay(500 * (i+1), CancellationToken.None);  
                        Directory.Delete(workspace, recursive: true);
                    }
                    break;
                } 
                catch
                {
                    // Ignore cleanup failures 
                }
            }
        }
    }

    private static void WriteMarkdownReport(AgentRunConfig config, AgentMetadata metadata)
    {
        var filePath = BuildReportFilePath(config);
        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

        var markdown = GenerateMarkdownReport(config, metadata);
        markdown = RedactSecrets(markdown);

        File.WriteAllText(filePath, markdown, Encoding.UTF8);
    }

    private static string GenerateMarkdownReport(AgentRunConfig config, AgentMetadata metadata)
    {
        var lines = new List<string>();

        lines.Add("# User Prompt");
        lines.Add("");
        lines.Add(config.Prompt);
        lines.Add("");

        lines.Add("# Assistant");
        lines.Add("");

        // Store tool results keyed by toolCallId
        var toolResults = new Dictionary<string, (bool success, string? content, string? error)>();

        foreach (var evt in metadata.Events)
        {
            if (evt.Type == "tool.execution_complete")
            {
                var toolCallId = evt.Data.TryGetValue("toolCallId", out var idObj) ? idObj?.ToString() : null;
                if (string.IsNullOrEmpty(toolCallId)) continue;

                var success = evt.Data.TryGetValue("success", out var isSuccess) && isSuccess is bool b && b;
                var content = evt.Data.TryGetValue("content", out var contentObj) ? contentObj?.ToString() : null;
                var error = evt.Data.TryGetValue("error", out var errorObj) ? errorObj?.ToString() : null;

                toolResults[toolCallId] = (success, content, error);
            }
        }

        foreach (var evt in metadata.Events)
        {
            switch (evt.Type)
            {
                case "assistant.message":
                    if (evt.Data.TryGetValue("content", out var contentObject) && contentObject is string messageContent && messageContent.Length > 0)
                    {
                        lines.Add(messageContent);
                        lines.Add("");
                    }
                    break;

                case "assistant.reasoning":
                    if (evt.Data.TryGetValue("content", out var reasoningObject) && reasoningObject is string reasoning && reasoning.Length > 0)
                    {
                        lines.Add("> Reasoning:");
                        lines.Add("> " + reasoning.Replace("\n", "\n> "));
                        lines.Add("");
                    }
                    break;

                case "tool.execution_start":
                    {
                        var toolName = evt.Data.TryGetValue("toolName", out var toolNameValue) ? toolNameValue?.ToString() : "unknown";
                        var toolCallId = evt.Data.TryGetValue("toolCallId", out var toolCallIdValue) ? toolCallIdValue?.ToString() : "";

                        evt.Data.TryGetValue("arguments", out var args);
                        var argsJson = AgentRunnerUtils.SafeJson(args);

                        lines.Add("```");
                        lines.Add($"tool: {toolName}");
                        lines.Add($"arguments: {argsJson}");

                        if (!string.IsNullOrEmpty(toolCallId) && toolResults.TryGetValue(toolCallId, out var result))
                        {
                            if (result.success && !string.IsNullOrEmpty(result.content))
                                lines.Add($"response: {Truncate(result.content!, 1000)}");
                            else if (!result.success && !string.IsNullOrEmpty(result.error))
                                lines.Add($"error: {Truncate(result.error!, 1000)}");
                        }

                        lines.Add("```");
                        lines.Add("");
                        break;
                    }

                case "session.error":
                    {
                        var message = evt.Data.TryGetValue("message", out var messageObj) ? messageObj?.ToString() : "unknown error";
                        var errorType = evt.Data.TryGetValue("errorType", out var errorObj) ? errorObj?.ToString() : "unknown";
                        var statusCode = evt.Data.TryGetValue("statusCode", out var statusObj) ? statusObj?.ToString() : "unknown";
                        lines.Add("```");
                        lines.Add($"session.error: {errorType}");
                        lines.Add($"message: {message}");
                        lines.Add($"statusCode: {statusCode}");
                        lines.Add("```");
                        lines.Add("");
                        break;
                    }
            }
        }

        return string.Join("\n", lines);
    }

    private static string RedactSecrets(string text)
    {
        var result = text;
        result = JwtPattern().Replace(result, "[REDACTED]");
        result = BearerPattern().Replace(result, "[REDACTED]");
        result = SecretKeyValuePattern().Replace(result, "[REDACTED]");
        return result;
    }

    private static string BuildReportFilePath(AgentRunConfig config)
    {
        var runDir = $"test-run-{TimeStamp}";
        var ns = config.Namespace ?? "unknown";
        var tool = config.ToolName ?? $"test-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";
        var file = $"{tool}-{DateTimeOffset.UtcNow:HHmmssfff}.md";
        return Path.Combine(DefaultReportDir, runDir, ns, file);
    }

    private static string CreateTempWorkspace(string prefix)
    {
        var dir = Path.Combine(Path.GetTempPath(), prefix + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        return dir;
    }

    /// <summary>
    /// Builds the MCP servers configuration based on whether an HTTP URL is provided.
    /// If a URL is provided, uses HTTP transport to connect to a shared server.
    /// Otherwise, spawns a new stdio process per session.
    /// </summary>
    // private static Dictionary<string, object> BuildMcpServersConfig(string? httpServerUrl)
    // {
    //     if (!string.IsNullOrEmpty(httpServerUrl))
    //     {
    //         // The MCP server exposes SSE at /sse endpoint
    //         // Ensure URL ends with /sse for SSE transport
    //         var sseUrl = httpServerUrl.TrimEnd('/');
    //         if (!sseUrl.EndsWith("/sse", StringComparison.OrdinalIgnoreCase))
    //         {
    //             sseUrl += "/sse";
    //         }
            
    //         return new Dictionary<string, object>
    //         {
    //             ["azure"] = new Dictionary<string, object>
    //             {
    //                 ["type"] = "sse",
    //                 ["url"] = sseUrl,
    //                 ["tools"] = new List<string> { "*" }
    //             }
    //         };
    //     }

    //     // stdio mode unchanged
    //     return new Dictionary<string, object>
    //     {
    //         ["azure"] = new McpLocalServerConfig
    //         {
    //             Type = "stdio",
    //             Command = "npx",
    //             Args = ["-y", "@azure/mcp", "server", "start"],
    //             Tools = ["*"]
    //         }
    //     };
    // }

    private static bool TryMapEvent(object ev, out AgentSessionEvent mapped)
    {
        // Session idle
        if (ev is SessionIdleEvent)
        {
            mapped = new AgentSessionEvent { Type = "session.idle" };
            return true;
        }

        // Assistant final message
        if (ev is AssistantMessageEvent msg)
        {
            mapped = new AgentSessionEvent
            {
                Type = "assistant.message",
                Data = new Dictionary<string, object?>
                {
                    ["content"] = msg.Data.Content,
                    ["messageId"] = msg.Data.MessageId,
                }
            };
            return true;
        }

        // Assistant message delta
        if (ev is AssistantMessageDeltaEvent msgDelta)
        {
            mapped = new AgentSessionEvent
            {
                Type = "assistant.message_delta",
                Data = new Dictionary<string, object?>
                {
                    ["deltaContent"] = msgDelta.Data.DeltaContent,
                    ["messageId"] = msgDelta.Data.MessageId,
                }
            };
            return true;
        }

        // Reasoning
        if (ev is AssistantReasoningEvent reasoning)
        {
            mapped = new AgentSessionEvent
            {
                Type = "assistant.reasoning",
                Data = new Dictionary<string, object?>
                {
                    ["content"] = reasoning.Data.Content,
                    ["reasoningId"] = reasoning.Data.ReasoningId,
                }
            };
            return true;
        }

        if (ev is AssistantReasoningDeltaEvent reasoningDelta)
        {
            mapped = new AgentSessionEvent
            {
                Type = "assistant.reasoning_delta",
                Data = new Dictionary<string, object?>
                {
                    ["deltaContent"] = reasoningDelta.Data.DeltaContent,
                    ["reasoningId"] = reasoningDelta.Data.ReasoningId,
                }
            };
            return true;
        }

        // Tool start
        if (ev is ToolExecutionStartEvent toolStart)
        {
            mapped = new AgentSessionEvent
            {
                Type = "tool.execution_start",
                Data = new Dictionary<string, object?>
                {
                    ["toolName"] = toolStart.Data.ToolName,
                    ["toolCallId"] = toolStart.Data.ToolCallId,
                    ["arguments"] = toolStart.Data.Arguments,
                    ["mcpToolName"] = toolStart.Data.McpToolName,
                }
            };
            return true;
        }

        // Tool complete
        if (ev is ToolExecutionCompleteEvent toolComplete)
        {
            mapped = new AgentSessionEvent
            {
                Type = "tool.execution_complete",
                Data = new Dictionary<string, object?>
                {
                    ["toolCallId"] = toolComplete.Data.ToolCallId,
                    ["success"] = toolComplete.Data.Success,
                    ["content"] = toolComplete.Data.Result?.Content,
                    ["error"] = toolComplete.Data.Error?.Message,
                }
            };
            return true;
        }

        // Session error
        if (ev is SessionErrorEvent sessionError)
        {
            mapped = new AgentSessionEvent
            {
                Type = "session.error",
                Data = new Dictionary<string, object?>
                {
                    ["errorType"] = sessionError.Data.ErrorType,
                    ["message"] = sessionError.Data.Message,
                    ["statusCode"] = sessionError.Data.StatusCode,                
                }
            };
            return true;
        }

        mapped = default!;
        return false;
    }

    private static string Truncate(string s, int max) => s.Length <= max ? s : s[..max] + "... (truncated)";

    /// <summary>
    /// Returns the set of currently running azmcp.exe process IDs.
    /// </summary>
    // private static HashSet<int> GetAzmcpProcessIds()
    // {
    //     try
    //     {
    //         return Process.GetProcessesByName("azmcp")
    //             .Select(p => { using (p) return p.Id; })
    //             .ToHashSet();
    //     }
    //     catch
    //     {
    //         return [];
    //     }
    // }

    /// <summary>
    /// Kills azmcp.exe processes that were spawned after the pre-session snapshot.
    /// This catches processes leaked by the SDK when session.AbortAsync/DisposeAsync
    /// fails to terminate the child stdio process.
    /// </summary>
    // private static void KillLeakedAzmcpProcesses(HashSet<int> preSessionPids)
    // {
    //     try
    //     {
    //         foreach (var process in Process.GetProcessesByName("azmcp"))
    //         {
    //             using (process)
    //             {
    //                 if (!preSessionPids.Contains(process.Id))
    //                 {
    //                     try
    //                     {
    //                         process.Kill(entireProcessTree: true);
    //                         process.WaitForExit(3000);
    //                     }
    //                     catch
    //                     {
    //                         // Process may have already exited
    //                     }
    //                 }
    //             }
    //         }
    //     }
    //     catch
    //     {
    //         // Ignore errors during cleanup
    //     }
    // }

    /// <summary>
    /// Kills all running azmcp.exe processes. Used as a final safety net
    /// at program exit to ensure no orphaned processes remain.
    /// </summary>
    // public static void KillAllAzmcpProcesses()
    // {
    //     try
    //     {
    //         foreach (var process in Process.GetProcessesByName("azmcp"))
    //         {
    //             using (process)
    //             {
    //                 try
    //                 {
    //                     process.Kill(entireProcessTree: true);
    //                     process.WaitForExit(3000);
    //                 }
    //                 catch
    //                 {
    //                     // Process may have already exited
    //                 }
    //             }
    //         }
    //     }
    //     catch
    //     {
    //         // Ignore errors during cleanup
    //     }
    // }

}
