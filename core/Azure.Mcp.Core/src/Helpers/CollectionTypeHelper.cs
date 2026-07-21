// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;

namespace Azure.Mcp.Core.Helpers;

/// <summary>
/// Helper class for determining collection type characteristics
/// </summary>
public static class CollectionTypeHelper
{
    /// <summary>
    /// Determines if a type should be treated as an array/collection for JSON schema and command-line parsing
    /// </summary>
    /// <param name="type">The type to check</param>
    /// <returns>True if the type should be treated as an array, false otherwise</returns>
    public static bool IsArrayType(Type type)
    {
        ArgumentNullException.ThrowIfNull(type);

        // Handle nullable types
        var effectiveType = Nullable.GetUnderlyingType(type) ?? type;

        // String is IEnumerable<char> but should not be treated as an array
        if (effectiveType == typeof(string))
        {
            return false;
        }

        // Check if it's a collection type
        if (typeof(IEnumerable).IsAssignableFrom(effectiveType))
        {
            return !IsDictionaryType(effectiveType);
        }

        return false;
    }

    /// <summary>
    /// Determines if a type is a dictionary type that should be treated as an object
    /// </summary>
    /// <param name="type">The type to check</param>
    /// <returns>True if the type is a dictionary type, false otherwise</returns>
    public static bool IsDictionaryType(Type type)
    {
        ArgumentNullException.ThrowIfNull(type);

        // Handle nullable types
        var effectiveType = Nullable.GetUnderlyingType(type) ?? type;

        // Check for dictionary types in an AOT-safe way
        var isDictionary = typeof(IDictionary).IsAssignableFrom(effectiveType);

        // Also check for common generic dictionary types
        if (!isDictionary && effectiveType.IsGenericType)
        {
            var genericTypeDef = effectiveType.GetGenericTypeDefinition();
            isDictionary = genericTypeDef == typeof(IDictionary<,>) ||
                          genericTypeDef == typeof(Dictionary<,>) ||
                          genericTypeDef == typeof(SortedDictionary<,>);
        }

        return isDictionary;
    }
}
