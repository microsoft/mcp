// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization.Metadata;

namespace Microsoft.Mcp.Core.Commands;

internal sealed class ResponseFieldSelection(JsonTypeInfo typeInfo, string? collectionProperty, bool isCollection)
{
    private readonly string[] _schemaPath = collectionProperty is not null
        ? ["properties", collectionProperty, "items"]
        : isCollection ? ["items"] : [];

    private readonly HashSet<string> _fields = new(
        typeInfo.Properties.Where(p => p.Get is not null).Select(p => p.Name), StringComparer.OrdinalIgnoreCase);

    internal ReadOnlySpan<string> SchemaPath => _schemaPath;

    internal string DescriptionSuffix { get; } =
        $"Choices are: {string.Join(", ", typeInfo.Properties.Where(p => p.Get is not null).Select(p => p.Name))}";

    internal static ResponseFieldSelection Create(JsonTypeInfo typeInfo)
    {
        string? collectionProperty = null;
        var isCollection = false;
        // A one-property wrapper (for example, { "blobs": [...] }) selects fields from
        // each array item, but retains the wrapper so the response shape does not change.
        if (typeInfo.Kind == JsonTypeInfoKind.Object && typeInfo.Properties.Count == 1)
        {
            var property = typeInfo.Properties[0];
            var propertyTypeInfo = typeInfo.Options.GetTypeInfo(property.PropertyType);
            if (propertyTypeInfo.Kind == JsonTypeInfoKind.Enumerable)
            {
                collectionProperty = property.Name;
                typeInfo = propertyTypeInfo;
            }
        }

        if (typeInfo.Kind == JsonTypeInfoKind.Enumerable && typeInfo.ElementType is { } elementType)
        {
            isCollection = true;
            typeInfo = typeInfo.Options.GetTypeInfo(elementType);
        }

        // Use the serializer's metadata rather than CLR member names so naming policies and
        // JsonPropertyName attributes match the names that clients actually receive.
        if (typeInfo.Kind != JsonTypeInfoKind.Object)
        {
            throw new InvalidOperationException("Field selection requires an object response or a collection of objects.");
        }

        return new(typeInfo, collectionProperty, isCollection);
    }

    internal HashSet<string>? Parse(string? select)
    {
        if (select is null)
        {
            return null;
        }

        var fields = select.Split(',', StringSplitOptions.TrimEntries).ToHashSet(StringComparer.OrdinalIgnoreCase);
        if (fields.Any(field => !_fields.Contains(field)))
        {
            throw new CommandValidationException(
                $"Invalid --select value. {DescriptionSuffix}", HttpStatusCode.BadRequest);
        }

        return fields;
    }

    internal JsonNode? Apply(JsonNode? result, HashSet<string> fields)
    {
        // SerializeToNode uses the same JsonTypeInfo as the response writer. Filtering this
        // private tree avoids mutating the command's result object or caching fields across calls.
        var target = collectionProperty is null ? result : result?[collectionProperty];
        if (target is null)
        {
            return result;
        }

        if (isCollection)
        {
            if (target is not JsonArray items)
            {
                throw new JsonException("The response does not match the collection metadata used for field selection.");
            }

            foreach (var item in items)
            {
                FilterObject(item, fields);
            }
        }
        else
        {
            FilterObject(target, fields);
        }

        return result;
    }

    private static void FilterObject(JsonNode? node, HashSet<string> fields)
    {
        if (node is null)
        {
            return;
        }

        if (node is not JsonObject obj)
        {
            throw new JsonException("The response does not match the object metadata used for field selection.");
        }

        foreach (var name in obj.Select(property => property.Key).Where(name => !fields.Contains(name)).ToArray())
        {
            // Materialize the names before removal because JsonObject cannot be modified while enumerating.
            obj.Remove(name);
        }
    }
}
