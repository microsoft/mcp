// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Nodes;
using ModelContextProtocol.Protocol;

namespace Microsoft.Mcp.Core.Helpers;

/// <summary>
/// Helper methods for dealing with ModelContextProtocol (MCP) related operations, such as injecting tool information
/// into call tool results, configuring custom '_meta' fields, etc.
/// </summary>
public static class McpHelper
{
    public const string SecretHintMetaKey = "SecretHint";
    public const string LocalRequiredHintMetaKey = "LocalRequiredHint";
    public const string ToolIdMetaKey = "MicrosoftMcpToolId";

    /// <summary>
    /// Determines whether the tool has the hint in its metadata and is true.
    /// </summary>
    /// <param name="tool">The tool to check its metadata for the hint.</param>
    /// <returns>True if the hint was found, successfully extracted, and is true; otherwise, false.</returns>
    public static bool HasHint(Tool tool, string hintKey)
        => tool.Meta != null && tool.Meta.TryGetPropertyValue(hintKey, out var hintNode)
            && hintNode is JsonValue hintValue
            && hintValue.TryGetValue<bool>(out var hint)
            && hint;

    /// <summary>
    /// Injects the tool ID into the metadata of a CallToolResult for better traceability in MCP interactions.
    /// </summary>
    /// <param name="result">The CallToolResult to enrich.</param>
    /// <param name="toolId">The unique identifier of the tool being called.</param>
    /// <returns>The enriched CallToolResult with the tool ID metadata injected.</returns>
    public static CallToolResult InjectToolIdMetadata(CallToolResult result, string toolId)
    {
        result.Meta ??= [];
        result.Meta[ToolIdMetaKey] = toolId;
        return result;
    }

    /// <summary>
    /// Attempts to inject the tool ID from the '_meta' field of another MCP spec class, if it exists, into the call
    /// tool result.
    /// </summary>
    /// <param name="result">The CallToolResult to enrich.</param>
    /// <param name="meta">The '_meta' field from another MCP spec class that may contain the tool ID.</param>
    /// <returns>The enriched CallToolResult with the tool ID metadata injected, if the tool ID was found in the provided meta; otherwise, the original CallToolResult.</returns>
    public static CallToolResult InjectToolIdMetadata(CallToolResult result, JsonObject? meta)
    {
        if (meta != null && meta.TryGetPropertyValue(ToolIdMetaKey, out var toolIdNode)
            && toolIdNode is JsonValue toolIdValue
            && toolIdValue.TryGetValue<string>(out var toolId))
        {
            result.Meta ??= [];
            result.Meta[ToolIdMetaKey] = toolId;
        }
        return result;
    }
}