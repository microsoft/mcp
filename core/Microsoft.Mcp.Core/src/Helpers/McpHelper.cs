// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

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

    // In the MCP 2026-07-28 stateless protocol there is no initialize handshake, so
    // request.Server.ClientInfo is null for every request. Clients that support the new
    // spec embed their identity in each request's _meta field under this key.
    public const string ClientInfoMetaKey = "io.modelcontextprotocol/clientInfo";
    public const string ClientInfoNameKey = "name";
    public const string ClientInfoVersionKey = "version";

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
    public static CallToolResult InjectToolIdMetadata(CallToolResult result, string? toolId)
    {
        if (toolId != null)
        {
            result.Meta ??= [];
            result.Meta[ToolIdMetaKey] = toolId;
        }
        return result;
    }

    /// <summary>
    /// Attempts to extract the tool ID from the '_meta' field of a given JsonObject, if it exists.
    /// </summary>
    /// <param name="meta">The '_meta' object to extract from.</param>
    /// <returns>The extracted tool ID, or null if it wasn't present.</returns>
    public static string? GetToolIdFromMeta(JsonObject? meta)
    {
        if (meta != null && meta.TryGetPropertyValue(ToolIdMetaKey, out var toolIdNode)
            && toolIdNode is JsonValue toolIdValue
            && toolIdNode.GetValueKind() == JsonValueKind.String
            && toolIdValue.TryGetValue<string>(out var toolId))
        {
            return toolId;
        }
        return null;
    }

    /// <summary>
    /// Creates a telemetry-friendly string array representation of the tool's parameter names.
    /// </summary>
    /// <param name="request">The tool call request parameters.</param>
    /// <returns>A string array representing the parameter names, or null if there wasn't parameters.</returns>
    public static string? CreateToolParametersTelemetry(RequestContext<CallToolRequestParams> request)
    {
        if (request?.Params?.Arguments == null || request.Params.Arguments.Count == 0)
        {
            return null;
        }

        return JsonSerializer.Serialize(request.Params.Arguments.Keys.ToList(), ModelsJsonContext.Default.ListString);
    }

    /// <summary>
    /// Creates a telemetry-friendly string representation of the tool's annotations.
    /// </summary>
    /// <param name="tool">The MCP tool definition.</param>
    /// <returns>A telemetry-friendly string of the tool's annotations.</returns>
    public static string CreateToolAnnotationTelemetry(Tool tool) => JsonSerializer.Serialize(new()
    {
        ["destructive"] = tool.Annotations?.DestructiveHint ?? false,
        ["idempotent"] = tool.Annotations?.IdempotentHint ?? false,
        ["openworld"] = tool.Annotations?.OpenWorldHint ?? false,
        ["readonly"] = tool.Annotations?.ReadOnlyHint ?? false,
        ["secret"] = HasHint(tool, SecretHintMetaKey),
        ["localrequired"] = HasHint(tool, LocalRequiredHintMetaKey),
    }, ModelsJsonContext.Default.DictionaryStringBoolean);

    /// <summary>
    /// Creates a telemetry-friendly string representation of the IBaseCommand-based tool.
    /// </summary>
    /// <param name="command">The IBaseCommand-based tool.</param>
    /// <returns>A telemetry-friendly string of the IBaseCommand-based tool.</returns>
    public static string CreateToolAnnotationTelemetry(IBaseCommand command) => JsonSerializer.Serialize(new()
    {
        ["destructive"] = command.Metadata.Destructive,
        ["idempotent"] = command.Metadata.Idempotent,
        ["openworld"] = command.Metadata.OpenWorld,
        ["readonly"] = command.Metadata.ReadOnly,
        ["secret"] = command.Metadata.Secret,
        ["localrequired"] = command.Metadata.LocalRequired,
    }, ModelsJsonContext.Default.DictionaryStringBoolean);
}
