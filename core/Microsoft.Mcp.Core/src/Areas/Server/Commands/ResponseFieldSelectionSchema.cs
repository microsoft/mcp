// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Mcp.Core.Areas.Server.Commands.ToolLoading;

namespace Microsoft.Mcp.Core.Areas.Server.Commands;

internal static class ResponseFieldSelectionSchema
{
    internal static JsonObject Apply(JsonObject schema, ReadOnlySpan<string> selectionPath, bool wrapped)
    {
        if (wrapped)
        {
            // Exported references are relative to the result, before MCP adds its value wrapper.
            RebaseReferences(schema["properties"]![StructuredOutputHelper.ValuePropertyName]!,
                $"#/properties/{StructuredOutputHelper.ValuePropertyName}");
            string[] wrappedPath = ["properties", StructuredOutputHelper.ValuePropertyName, .. selectionPath];
            selectionPath = wrappedPath;
        }

        var originalSchema = FindReferences(schema).Any() ? schema.DeepClone() : null;
        var selectedSchema = MakeFieldsOptional(schema, selectionPath, originalSchema ?? schema) as JsonObject
            ?? throw new JsonException("Field selection requires an object output schema.");

        if (originalSchema is not null)
        {
            // Recursive/reused schemas must still enforce the original requirements in nested values.
            // Keep references pointed at an unmodified definition, not at the relaxed selection level.
            var definitions = selectedSchema["$defs"] as JsonObject ?? new JsonObject();
            var definitionName = "unselectedResult";
            while (definitions.ContainsKey(definitionName))
            {
                definitionName += "_";
            }

            var prefix = $"#/$defs/{definitionName}";
            RebaseReferences(selectedSchema, prefix);
            RebaseReferences(originalSchema, prefix);
            definitions[definitionName] = originalSchema;
            if (selectedSchema["$defs"] is null)
            {
                selectedSchema["$defs"] = definitions;
            }
        }

        return selectedSchema;
    }

    private static JsonNode MakeFieldsOptional(JsonNode node, ReadOnlySpan<string> path, JsonNode originalSchema)
    {
        if (node is not JsonObject schema)
        {
            return node;
        }

        if (schema["$ref"] is JsonValue reference && reference.TryGetValue<string>(out var pointer))
        {
            var expanded = MakeFieldsOptional(ResolveReference(originalSchema, pointer).DeepClone(), path, originalSchema);
            schema.Remove("$ref");
            return schema.Count == 0 ? expanded : new JsonObject
            {
                ["allOf"] = new JsonArray(expanded, MakeFieldsOptional(schema.DeepClone(), path, originalSchema)),
            };
        }

        if (path.IsEmpty)
        {
            schema.Remove("required");
        }
        else if (path[0] == "properties" && schema["properties"] is JsonObject properties
            && properties[path[1]] is { } property)
        {
            properties[path[1]] = MakeFieldsOptional(property, path[2..], originalSchema);
        }
        else if (path[0] == "items" && schema["items"] is { } items)
        {
            schema["items"] = MakeFieldsOptional(items, path[1..], originalSchema);
        }

        foreach (var keyword in new[] { "anyOf", "oneOf", "allOf" })
        {
            if (schema[keyword] is JsonArray variants)
            {
                for (var index = 0; index < variants.Count; index++)
                {
                    if (variants[index] is { } variant)
                    {
                        variants[index] = MakeFieldsOptional(variant, path, originalSchema);
                    }
                }
            }
        }

        return schema;
    }

    private static JsonNode ResolveReference(JsonNode root, string pointer)
    {
        if (pointer == "#")
        {
            return root;
        }

        if (pointer.StartsWith("#/", StringComparison.Ordinal))
        {
            var node = root;
            foreach (var token in Uri.UnescapeDataString(pointer[2..]).Split('/'))
            {
                var name = token.Replace("~1", "/", StringComparison.Ordinal).Replace("~0", "~", StringComparison.Ordinal);
                node = node switch
                {
                    JsonObject obj => obj[name],
                    JsonArray array when int.TryParse(name, out var index) && index >= 0 && index < array.Count => array[index],
                    _ => null,
                } ?? throw new JsonException($"Cannot resolve output schema reference '{pointer}'.");
            }

            return node;
        }

        throw new JsonException($"Unsupported output schema reference '{pointer}'.");
    }

    private static void RebaseReferences(JsonNode schema, string prefix)
    {
        foreach (var reference in FindReferences(schema).ToArray())
        {
            reference["$ref"] = prefix + reference["$ref"]!.GetValue<string>()[1..];
        }
    }

    private static IEnumerable<JsonObject> FindReferences(JsonNode? node)
    {
        if (node is JsonObject obj)
        {
            if (obj["$ref"] is JsonValue value && value.TryGetValue<string>(out var pointer)
                && pointer.StartsWith('#'))
            {
                yield return obj;
            }

            foreach (var property in obj)
            {
                foreach (var reference in FindReferences(property.Value))
                {
                    yield return reference;
                }
            }
        }
        else if (node is JsonArray array)
        {
            foreach (var item in array)
            {
                foreach (var reference in FindReferences(item))
                {
                    yield return reference;
                }
            }
        }
    }
}
