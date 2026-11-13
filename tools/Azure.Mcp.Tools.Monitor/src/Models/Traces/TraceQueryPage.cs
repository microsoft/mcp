// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Serialization;

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
            if (!Properties.TryGetValue(property.Key, out var current) ||
                (property.Key is "hasLimitReached" or "isPartialResult" or "isSystemLimitHit" &&
                 property.Value.ValueKind == JsonValueKind.True && current.ValueKind == JsonValueKind.False))
            {
                Properties[property.Key] = property.Value;
            }
        }
    }
}
