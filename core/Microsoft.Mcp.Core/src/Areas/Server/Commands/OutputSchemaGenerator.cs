// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Microsoft.Mcp.Core.Areas.Server.Models;

namespace Microsoft.Mcp.Core.Areas.Server.Commands;

/// <summary>
/// Generates a <see cref="ToolOutputSchema"/> from a <see cref="JsonTypeInfo"/>,
/// reflecting the shape of a command's result type for use as <c>outputSchema</c>
/// on MCP tool definitions.
/// </summary>
public static class OutputSchemaGenerator
{
    /// <summary>
    /// Generates a JSON Schema element from the given <see cref="JsonTypeInfo"/>.
    /// The schema includes top-level properties with their JSON types.
    /// Nested object types are represented as <c>"object"</c> without further expansion.
    /// </summary>
    /// <param name="typeInfo">The source-generated type info for the result type.</param>
    /// <returns>A <see cref="JsonElement"/> representing the output JSON schema.</returns>
    public static JsonElement Generate(JsonTypeInfo typeInfo)
    {
        ArgumentNullException.ThrowIfNull(typeInfo);

        var schema = new ToolOutputSchema();

        if (typeInfo.Kind == JsonTypeInfoKind.Object)
        {
            foreach (var prop in typeInfo.Properties)
            {
                schema.Properties[prop.Name] = TypeToJsonTypeMapper.CreatePropertySchema(
                    prop.PropertyType,
                    description: null);
            }
        }

        return JsonSerializer.SerializeToElement(schema, ServerJsonContext.Default.ToolOutputSchema);
    }
}
