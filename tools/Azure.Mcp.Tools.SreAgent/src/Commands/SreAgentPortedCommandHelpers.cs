// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization.Metadata;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.SreAgent.Commands;

internal static class SreAgentPortedCommandHelpers
{
    public static void SetTextResult(CommandResponse response, string message)
    {
        response.Results = ResponseResult.Create(new(message), SreAgentJsonContext.Default.SreAgentTextResult);
    }

    public static List<T> DeserializeArray<T>(string json, JsonTypeInfo<List<T>> jsonTypeInfo)
    {
        using var document = JsonDocument.Parse(string.IsNullOrWhiteSpace(json) ? "[]" : json);
        var root = document.RootElement;
        if (root.ValueKind == JsonValueKind.Array)
        {
            return JsonSerializer.Deserialize(root.GetRawText(), jsonTypeInfo) ?? [];
        }

        if (root.ValueKind == JsonValueKind.Object)
        {
            foreach (var propertyName in new[] { "value", "items", "documents", "data", "results", "files" })
            {
                if (root.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Array)
                {
                    return JsonSerializer.Deserialize(property.GetRawText(), jsonTypeInfo) ?? [];
                }
            }
        }

        return [];
    }

    public static string SanitizeKebabCase(string name)
    {
        var chars = name.Trim().ToLowerInvariant().Select(c => char.IsAsciiLetterOrDigit(c) ? c : '-').ToArray();
        var collapsed = string.Join('-', new string(chars).Split('-', StringSplitOptions.RemoveEmptyEntries));
        return string.IsNullOrWhiteSpace(collapsed) ? "agent" : collapsed;
    }

    public static string SanitizeFileName(string name)
    {
        var chars = name.Select(c => char.IsAsciiLetterOrDigit(c) || c is '.' or '_' or '-' ? c : '-').ToArray();
        return new string(chars);
    }

    public static JsonObject ConnectorPayload(string name, string dataSource, JsonObject extendedProperties) => new()
    {
        ["name"] = name,
        ["properties"] = new JsonObject
        {
            ["name"] = name,
            ["dataConnectorType"] = "Mcp",
            ["dataSource"] = dataSource,
            ["identity"] = string.Empty,
            ["extendedProperties"] = extendedProperties
        }
    };
}
