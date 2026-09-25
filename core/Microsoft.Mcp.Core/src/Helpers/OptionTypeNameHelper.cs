// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Mcp.Core.Helpers;

/// <summary>
/// Maps a command-line option's CLR value type to the JSON Schema type keyword
/// (<c>string</c>, <c>integer</c>, <c>number</c>, <c>boolean</c>, or <c>array</c>) reported
/// in the CLI tool metadata surfaced by <c>tools list</c> and <c>--learn</c>.
/// </summary>
/// <remarks>
/// The keyword set matches the MCP <c>inputSchema</c> produced by <c>OptionSchemaGenerator</c>,
/// so the CLI metadata and MCP tool schema stay consistent. Nullable value types are unwrapped
/// (nullability is conveyed separately by the option's <c>required</c> flag), enums are reported
/// as <c>string</c>, and any collection type is reported as <c>array</c>.
/// </remarks>
public static class OptionTypeNameHelper
{
    private const string StringType = "string";
    private const string IntegerType = "integer";
    private const string NumberType = "number";
    private const string BooleanType = "boolean";
    private const string ArrayType = "array";

    /// <summary>
    /// Returns the JSON Schema type keyword for the supplied option value type.
    /// </summary>
    /// <param name="valueType">The CLR value type of the option (for example, the command-line option's value type).</param>
    /// <returns>The JSON Schema type keyword describing the option.</returns>
    public static string GetJsonSchemaType(Type valueType)
    {
        ArgumentNullException.ThrowIfNull(valueType);

        // Collections (string[], int[], List<T>, etc.) map to "array". Checked before nullable
        // unwrapping because CollectionTypeHelper already handles Nullable and treats string as a
        // scalar rather than a char sequence.
        if (CollectionTypeHelper.IsArrayType(valueType))
        {
            return ArrayType;
        }

        // Unwrap Nullable<T>; the option's required flag conveys nullability separately.
        var type = Nullable.GetUnderlyingType(valueType) ?? valueType;

        // Enums are surfaced by their string name in CLI usage.
        if (type.IsEnum)
        {
            return StringType;
        }

        return Type.GetTypeCode(type) switch
        {
            TypeCode.Boolean => BooleanType,
            TypeCode.Byte or TypeCode.SByte
                or TypeCode.Int16 or TypeCode.UInt16
                or TypeCode.Int32 or TypeCode.UInt32
                or TypeCode.Int64 or TypeCode.UInt64 => IntegerType,
            TypeCode.Single or TypeCode.Double or TypeCode.Decimal => NumberType,
            _ => StringType,
        };
    }

    /// <summary>
    /// Returns the JSON Schema type keyword for the elements of a collection option, or
    /// <see langword="null"/> when the value type is not a collection.
    /// </summary>
    /// <param name="valueType">The CLR value type of the option.</param>
    /// <returns>
    /// The element type keyword (for example, <c>string</c> for <c>string[]</c>), or
    /// <see langword="null"/> for scalar options or collections whose element type cannot be resolved.
    /// </returns>
    public static string? GetElementJsonSchemaType(Type valueType)
    {
        ArgumentNullException.ThrowIfNull(valueType);

        if (!CollectionTypeHelper.IsArrayType(valueType))
        {
            return null;
        }

        var elementType = GetCollectionElementType(valueType);
        return elementType is null ? null : GetJsonSchemaType(elementType);
    }

    private static Type? GetCollectionElementType(Type type)
    {
        if (type.IsArray)
        {
            return type.GetElementType();
        }

        if (type.IsGenericType)
        {
            var genericDefinition = type.GetGenericTypeDefinition();
            if (genericDefinition == typeof(IEnumerable<>)
                || genericDefinition == typeof(ICollection<>)
                || genericDefinition == typeof(IList<>)
                || genericDefinition == typeof(IReadOnlyList<>)
                || genericDefinition == typeof(IReadOnlyCollection<>)
                || genericDefinition == typeof(List<>))
            {
                return type.GetGenericArguments()[0];
            }
        }

        return null;
    }
}
