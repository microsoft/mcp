// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Microsoft.Mcp.Core.Areas.Server.Models;
using Microsoft.Mcp.Core.Models.Command;

namespace Microsoft.Mcp.Core.Areas.Server.Commands;

/// <summary>
/// Generates a <see cref="ToolOutputSchema"/> from a <see cref="JsonTypeInfo"/>,
/// reflecting the shape of a command's result type for use as <c>outputSchema</c>
/// on MCP tool definitions.
/// </summary>
public static class OutputSchemaGenerator
{
    private static readonly Lazy<JsonElement> DefaultSchema = new(BuildDefaultSchema);

    /// <summary>
    /// Generates a JSON Schema element from the given <see cref="JsonTypeInfo"/>.
    /// When <paramref name="typeInfo"/> is provided the schema includes its top-level
    /// properties with their JSON types. When <paramref name="typeInfo"/> is <c>null</c>
    /// a default schema based on <see cref="CommandResponse"/> is returned.
    /// </summary>
    /// <param name="typeInfo">
    /// The source-generated type info for the result type, or <c>null</c> to use
    /// the default <see cref="CommandResponse"/> schema.
    /// </param>
    /// <returns>A <see cref="JsonElement"/> representing the output JSON schema.</returns>
    public static JsonElement Generate(JsonTypeInfo? typeInfo = null)
    {
        if (typeInfo is null)
        {
            return DefaultSchema.Value;
        }

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

    private static JsonElement BuildDefaultSchema()
    {
        var schema = new ToolOutputSchema();

        schema.Properties["status"] = new ToolPropertySchema { Type = "integer", Description = "HTTP status code." };
        schema.Properties["message"] = new ToolPropertySchema { Type = "string", Description = "Status or error message." };
        schema.Properties["results"] = new ToolPropertySchema { Type = "object", Description = "Command-specific result payload." };
        schema.Properties["duration"] = new ToolPropertySchema { Type = "integer", Description = "Execution duration in milliseconds." };

        return JsonSerializer.SerializeToElement(schema, ServerJsonContext.Default.ToolOutputSchema);
    }
}
