// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;
using Azure.Mcp.Core.Areas.Server.Models;
using Azure.Mcp.Core.Helpers;

namespace Azure.Mcp.Core.Areas.Server.Commands;

/// <summary>
/// Gets the JSON object type based on its C# type.
/// </summary>
public static class TypeToJsonTypeMapper
{
    private static readonly Dictionary<Type, string> s_typeToJsonMap = new()
    {
        // String types
        { typeof(string), "string" },
        { typeof(char), "string" },
        { typeof(Guid), "string" },
        { typeof(DateTime), "string" },
        { typeof(DateTimeOffset), "string" },
        { typeof(TimeSpan), "string" },
        { typeof(Uri), "string" },

        // Number types
        { typeof(int), "integer" },
        { typeof(uint), "integer" },
        { typeof(long), "integer" },
        { typeof(ulong), "integer" },
        { typeof(short), "integer" },
        { typeof(ushort), "integer" },
        { typeof(byte), "integer" },
        { typeof(sbyte), "integer" },

        { typeof(float), "number" },
        { typeof(double), "number" },
        { typeof(decimal), "number" },

        // Boolean
        { typeof(bool), "boolean" },

        // Arrays and collections
        { typeof(Array), "array" },

        // Object
        { typeof(object), "object" }
    };

    /// <summary>
    /// Gets the JSON type name based on the given type.  If <paramref name="type"/> is null, then "null" is returned.
    /// </summary>
    /// <param name="type">Type to get JSON type name from.</param>
    /// <returns></returns>
    public static string ToJsonType(this Type? type)
    {
        if (type == null)
        {
            return "null";
        }

        // Handle nullable types - treat them as the primitive they're based on
        var effectiveType = Nullable.GetUnderlyingType(type) ?? type;

        if (s_typeToJsonMap.TryGetValue(effectiveType, out string? jsonType) && jsonType != null)
        {
            return jsonType;
        }

        if (typeof(IEnumerable).IsAssignableFrom(type) && type != typeof(string))
        {
            return CollectionTypeHelper.IsDictionaryType(type) ? "object" : "array";
        }

        if (effectiveType.IsEnum)
        {
            return "integer";
        }

        return "object";
    }

    /// <summary>
    /// Gets the element type of an array or collection type.
    /// </summary>
    /// <param name="type">The array or collection type to analyze.</param>
    /// <returns>The element type if the type is an array or collection, otherwise null.</returns>
    public static Type? GetArrayOrCollectionElementType(Type type)
    {
        ArgumentNullException.ThrowIfNull(type);

        // Handle arrays
        if (type.IsArray)
        {
            return type.GetElementType();
        }

        // Handle generic collections like List<T>, IEnumerable<T>, etc.
        // Note: Dictionary types are handled as "object" in ToJsonType() so they won't reach this method
        if (type.IsGenericType && !type.IsGenericTypeDefinition)
        {
            var genericArgs = type.GetGenericArguments();

            if (genericArgs.Length == 1)
            {
                return genericArgs[0];
            }
        }

        // Handle non-generic IEnumerable or open generics
        if (typeof(IEnumerable).IsAssignableFrom(type) && type != typeof(string))
        {
            // Default to object
            return typeof(object);
        }

        return null;
    }

    /// <summary>
    /// Creates a JSON schema object for a given option type.
    /// </summary>
    /// <param name="optionType">The type of the option to create schema for.</param>
    /// <param name="description">The description for the option.</param>
    /// <returns>A JsonObject representing the JSON schema for the option.</returns>
    public static ToolPropertySchema CreatePropertySchema(Type optionType, string? description)
    {
        ArgumentNullException.ThrowIfNull(optionType);

        // Handle nullable types - get the underlying type for schema generation
        var effectiveType = Nullable.GetUnderlyingType(optionType) ?? optionType;
        var jsonType = effectiveType.ToJsonType();
        ToolPropertySchema? itemsSchema = null;

        // If the type is an array, we need to specify the items type recursively
        if (jsonType == "array")
        {
            var elementType = GetArrayOrCollectionElementType(effectiveType);

            if (elementType != null)
            {
                // Recursively create schema for nested arrays
                itemsSchema = CreateItemsSchema(elementType);
            }
        }

        return new ToolPropertySchema()
        {
            Type = jsonType,
            Description = description ?? string.Empty,
            Items = itemsSchema
        };
    }

    /// <summary>
    /// Creates a JSON schema object for items in an array or collection, handling nested arrays recursively.
    /// </summary>
    /// <param name="itemType">The type of the items in the array or collection.</param>
    /// <returns>A JsonObject representing the JSON schema for the array/collection items.</returns>
    private static ToolPropertySchema CreateItemsSchema(Type itemType)
    {
        ArgumentNullException.ThrowIfNull(itemType);

        // Handle nullable types for array items
        var effectiveType = Nullable.GetUnderlyingType(itemType) ?? itemType;
        var jsonType = effectiveType.ToJsonType();
        ToolPropertySchema? itemsSchema = null;

        // If the item type is also an array, recursively define its items
        if (jsonType == "array")
        {
            var nestedElementType = GetArrayOrCollectionElementType(effectiveType);

            if (nestedElementType != null)
            {
                itemsSchema = CreateItemsSchema(nestedElementType);
            }
        }

        return new ToolPropertySchema()
        {
            Type = jsonType,
            Items = itemsSchema
        };
    }
}
