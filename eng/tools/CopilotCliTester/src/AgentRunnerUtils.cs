// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using CopilotCliTester.Models;
using GitHub.Copilot.SDK;

namespace CopilotCliTester;

/// <summary>
/// Utility methods for agent runner operations
/// </summary>
internal static class AgentRunnerUtils
{
    // Internal/meta tools we do NOT want to count as "the expected MCP tool"
    private static readonly HashSet<string> IgnoredTools = new(StringComparer.OrdinalIgnoreCase)
    {
        "report_intent"
    };

    private static readonly string[] prefixes = new[] { "azure-", "azure_" };

    /// <summary>
    /// Returns tool.execution_start events
    /// </summary>
    public static IReadOnlyList<AgentSessionEvent> GetToolCalls(AgentMetadata metadata)
    {
        return metadata.Events
            .Where(e => e.Type == "tool.execution_start")
            .Where(e =>
            {
                var name = e.Data.TryGetValue("toolName", out var tn) ? tn?.ToString() : null;
                return !string.IsNullOrWhiteSpace(name) && !IgnoredTools.Contains(name!);
            }).ToList();
    }

    /// <summary>
    /// Check if the expected MCP tool was called during the agent session.
    /// </summary>
    public static bool WasToolInvoked(AgentMetadata metadata, string expectedTool)
    {
        return GetToolCalls(metadata).Any(evt =>
        {
            var resolved = ResolveToolName(evt);

            if (string.Equals(resolved, expectedTool, StringComparison.OrdinalIgnoreCase))
                return true;

            // Strip known single-segment namespace-proxy prefix instead of open-ended suffix match to avoid false positives (e.g., "subscription_list" matching "eventgrid_subscription_list")
            foreach (var prefix in prefixes)
            {
                if (resolved.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(resolved[prefix.Length..], expectedTool, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        });
    }

    /// <summary>
    /// Collect all tool names that were invoked during the session. Prefer mcpToolName, then arguments.command, then fall back to toolName.
    /// </summary>
    public static List<string> GetInvokedToolNames(AgentMetadata metadata)
    {
        return GetToolCalls(metadata)
            .Select(ResolveToolName)
            .Where(IsToolName)
            .ToList();
    }

    /// <summary>
    /// Returns true if the name looks like a tool identifier rather than a raw shell command. Tool names are alphanumeric with underscores, hyphens, and dots (e.g. "storage_account_list", "azure-storage", "grep_search").
    /// </summary>
    private static bool IsToolName(string name) =>
        name.Length > 0 && name.All(c => char.IsLetterOrDigit(c) || c is '_' or '-' or '.');

    private static string ResolveToolName(AgentSessionEvent evt)
    {
        // 1. Try arguments.command first (namespace-proxy wraps real command here)
        if (evt.Data.TryGetValue("arguments", out var argsObj) && argsObj is not null)
        {
            try
            {
                // Handle dictionary-like types
                if (argsObj is IReadOnlyDictionary<string, object?> dict && dict.TryGetValue("command", out var cmdVal) &&
                    cmdVal?.ToString() is string cmd && !string.IsNullOrWhiteSpace(cmd))
                {
                    return cmd;
                }

                // Handle JsonElement or string
                JsonDocument? doc = null;

                if (argsObj is JsonElement je)
                {
                    doc = JsonDocument.Parse(je.GetRawText());
                }
                else if (argsObj is string s)
                {
                    doc = JsonDocument.Parse(s);
                }
                else
                {
                    doc = JsonDocument.Parse(SafeJson(argsObj));
                }

                using (doc)
                {
                    if (doc.RootElement.TryGetProperty("command", out var cmdProp))
                    {
                        var command = cmdProp.GetString();
                        if (!string.IsNullOrWhiteSpace(command))
                            return command;
                    }
                }
            }
            catch
            {
                // fall through to mcpToolName / toolName
            }
        }

        // 2. Try mcpToolName
        if (evt.Data.TryGetValue("mcpToolName", out var mcp) && mcp is string mcpToolName && !string.IsNullOrWhiteSpace(mcpToolName))
            return mcpToolName;

        // 3. Fall back to toolName
        return evt.Data.TryGetValue("toolName", out var tn) ? tn?.ToString() ?? "unknown" : "unknown";
    }

    internal static string SafeJson(object? value)
    {
        try 
        { 
            return JsonSerializer.Serialize(value, JsonContext.Default.Object); 
        }
        catch 
        {
            return value?.ToString() ?? "null"; 
        }
    }

    public static string FindRepoRoot(string startingDir)
    {
        string? dir = startingDir;
        while (dir is not null)
        {
            if (File.Exists(Path.Combine(dir, "Microsoft.Mcp.slnx")) ||
                File.Exists(Path.Combine(dir, "mcp.sln")))
            {
                return dir;
            }
            dir = Path.GetDirectoryName(dir);
        }

        throw new InvalidOperationException(
            "Could not find repo root (directory containing Microsoft.Mcp.slnx). Make sure you're running from within the repo.");    
    }
}
