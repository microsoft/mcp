// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Mcp.Tools.Monitor.Commands;

namespace Azure.Mcp.Tools.Monitor.Models.Traces;

internal sealed class TraceQueryPage()
{
    public List<JsonElement>? Value { get; set; }

    // Validate continuation after saving rows, even when the server returns a malformed link.
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public JsonElement? NextLink { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement> Properties { get; set; } = [];

    [JsonIgnore]
    public bool HasMoreResults => NextLink switch
    {
        null or { ValueKind: JsonValueKind.Null } => false,
        { ValueKind: JsonValueKind.String } link => !string.IsNullOrWhiteSpace(link.GetString()),
        _ => throw new JsonException("Trace query results contain an invalid 'nextLink'.")
    };

    public void MergePropertiesFrom(TraceQueryPage? previousPage)
    {
        if (previousPage is null)
        {
            return;
        }

        foreach (var property in previousPage.Properties)
        {
            if (property.Key == "metadata" && property.Value.ValueKind == JsonValueKind.Object)
            {
                var currentMetadata = Properties.TryGetValue(property.Key, out var currentProperty)
                    && currentProperty.ValueKind == JsonValueKind.Object
                    ? currentProperty
                    : property.Value;
                var metadata = currentMetadata.EnumerateObject()
                    .ToDictionary(item => item.Name, item => item.Value, StringComparer.Ordinal);
                foreach (var item in property.Value.EnumerateObject())
                {
                    if (item.Name is "hasLimitReached" or "isPartialResult" or "isSystemLimitHit"
                        && item.Value.ValueKind == JsonValueKind.True)
                    {
                        metadata[item.Name] = item.Value;
                    }
                }
                Properties[property.Key] = JsonSerializer.SerializeToElement(metadata, MonitorJsonContext.Default.DictionaryStringJsonElement);
                continue;
            }

            if (!Properties.TryGetValue(property.Key, out var current) ||
                (property.Key is "hasLimitReached" or "isPartialResult" or "isSystemLimitHit" &&
                 property.Value.ValueKind == JsonValueKind.True && current.ValueKind == JsonValueKind.False))
            {
                Properties[property.Key] = property.Value;
            }
        }
    }
}
