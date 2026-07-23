// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Globalization;
using System.Text.Json.Nodes;
using Microsoft.Mcp.Core.Areas.Server.Options;
using Microsoft.Mcp.Core.Models.Command;

namespace Microsoft.Mcp.Core.Areas.Server.Commands.ToolLoading;

internal static class StructuredOutputHelper
{
    /// <summary>
    /// The first MCP protocol version that includes <c>outputSchema</c> and <c>structuredContent</c>.
    /// </summary>
    internal const string MinProtocolVersion = "2025-06-18";

    internal const string LegacyContentArgumentName = "legacy-content";
    internal const string CompactContentMessage =
        "Response successful. See structuredContent for details. If structuredContent is unavailable, retry with \"legacy-content\": true.";

    private const string LegacyContentArgumentDescription =
        "Return the complete response in content for clients that do not expose structuredContent.";
    private static readonly DateOnly MinProtocolDate = new(2025, 6, 18);

    internal static bool IsEnabled(StructuredOutputMode mode, string? negotiatedProtocolVersion) =>
        mode != StructuredOutputMode.Legacy && SupportsProtocolVersion(negotiatedProtocolVersion);

    internal static bool SupportsProtocolVersion(string? negotiatedProtocolVersion) =>
        DateOnly.TryParseExact(
            negotiatedProtocolVersion,
            "yyyy-MM-dd",
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out var protocolDate)
        && protocolDate >= MinProtocolDate;

    internal static void AddLegacyContentArgument(JsonObject inputSchema)
    {
        if (inputSchema["properties"] is not JsonObject properties)
        {
            properties = [];
            inputSchema["properties"] = properties;
        }

        if (properties.Any(property =>
            string.Equals(property.Key, LegacyContentArgumentName, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException(
                $"Command input schema already defines the reserved '{LegacyContentArgumentName}' argument.");
        }

        properties[LegacyContentArgumentName] = new JsonObject
        {
            ["type"] = "boolean",
            ["description"] = LegacyContentArgumentDescription,
            ["default"] = false
        };
    }

    internal static bool TryExtractLegacyContentArgument(
        IDictionary<string, JsonElement>? arguments,
        out IDictionary<string, JsonElement>? commandArguments,
        out bool legacyContent)
    {
        commandArguments = arguments;
        legacyContent = false;

        if (arguments is null)
        {
            return true;
        }

        string? argumentKey = null;
        foreach (var key in arguments.Keys)
        {
            if (!string.Equals(key, LegacyContentArgumentName, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (argumentKey is not null)
            {
                return false;
            }

            argumentKey = key;
        }

        if (argumentKey is null)
        {
            return true;
        }

        var value = arguments[argumentKey];
        if (value.ValueKind is not (JsonValueKind.True or JsonValueKind.False))
        {
            return false;
        }

        legacyContent = value.GetBoolean();
        var filteredArguments = new Dictionary<string, JsonElement>(arguments);
        filteredArguments.Remove(argumentKey);
        commandArguments = filteredArguments;
        return true;
    }

    /// <summary>
    /// Extracts and shapes the result payload from a serialized <see cref="CommandResponse"/>.
    /// Object payloads are used as-is; arrays and scalars are wrapped under <c>value</c>.
    /// </summary>
    internal static JsonElement? TryBuildStructuredContent(string jsonResponse)
    {
        using var document = JsonDocument.Parse(jsonResponse);

        if (!document.RootElement.TryGetProperty("results", out var results))
        {
            return null;
        }

        switch (results.ValueKind)
        {
            case JsonValueKind.Object:
                return results.Clone();
            case JsonValueKind.Null:
            case JsonValueKind.Undefined:
                return null;
            default:
                var wrapper = new JsonObject { ["value"] = JsonNode.Parse(results.GetRawText()) };
                return JsonSerializer.SerializeToElement(wrapper, ServerJsonContext.Default.JsonObject);
        }
    }
}
