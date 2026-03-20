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
        var toolStartEvents = GetToolCalls(metadata);

        return toolStartEvents.Any(evt =>
        {
            if (evt.Data.TryGetValue("mcpToolName", out var mcp) && mcp is string mcpToolName)
            {
                if (string.Equals(mcpToolName, expectedTool, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            var toolName = evt.Data.TryGetValue("toolName", out var tn) ? tn?.ToString() ?? "" : "";

            if (string.Equals(toolName, expectedTool, StringComparison.OrdinalIgnoreCase)) return true;

            // Namespace-prefixed match - helps when running local MCP server
            if (toolName.EndsWith($"_{expectedTool}", StringComparison.OrdinalIgnoreCase) ||
                toolName.EndsWith($"-{expectedTool}", StringComparison.OrdinalIgnoreCase))
                return true;

            // Check if tool appears as "command" parameter. e.g., tool="azure-workbooks" with arguments: { "command": "workbooks_create" }
            if (evt.Data.TryGetValue("arguments", out var argsObj) && argsObj is not null)
            {
                var argsJson = SafeJson(argsObj);
                if (argsJson.Contains($"\"command\":\"{expectedTool}\"", StringComparison.OrdinalIgnoreCase) ||
                    argsJson.Contains($"\"command\": \"{expectedTool}\"", StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        });
    }

    /// <summary>
    /// Collect all tool names that were invoked during the session.
    /// </summary>
    public static List<string> GetInvokedToolNames(AgentMetadata metadata)
    {
        return GetToolCalls(metadata)
            .Select(evt => evt.Data.TryGetValue("toolName", out var tn) ? tn?.ToString() ?? "unknown" : "unknown")
            .ToList();
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
}
